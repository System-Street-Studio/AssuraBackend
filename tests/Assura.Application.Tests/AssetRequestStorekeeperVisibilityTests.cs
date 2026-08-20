using Assura.Application.Features.AssetRequests.Queries;
using Assura.Application.Features.Requests.Commands;
using Assura.Application.Tests.Common;
using Assura.Domain.Constants;
using Assura.Domain.Entities;
using Assura.Domain.Enums;

namespace Assura.Application.Tests;

// Covers the BUGS.md Employee finding: "Employee cannot see any info from Storekeeper."
// AssetRequest had no column to record which Storekeeper processed a request or their
// remarks, so ProcessRequestCommand silently dropped that data for legacy AssetRequest
// rows (routed here via the negative-id convention) even though it recorded the same
// info correctly for the newer Request entity. These tests assert the Storekeeper's
// identity and remarks are now persisted and returned to the Employee via
// GetFilteredAssetRequestsQuery (the query behind GET /api/AssetRequests/employee/{id}
// and GET /api/AssetRequests/{id}, which the Employee frontend actually reads).
public class AssetRequestStorekeeperVisibilityTests
{
    [Fact]
    public async Task ProcessRequestCommand_OnLegacyAssetRequest_RecordsStorekeeperIdentityAndRemarks()
    {
        using var db = TestContextFactory.CreateContext();

        var storekeeper = new User
        {
            Id = 900,
            Username = "storekeeper",
            FirstName = "Store",
            LastName = "Keeper",
            Email = "store@example.com",
            PasswordHash = "x",
            Role = UserRole.Storekeeper
        };

        var assetRequest = new AssetRequest
        {
            Id = 700,
            AssetName = "Wireless Mouse",
            AssetCategory = "Peripherals",
            Priority = "Normal",
            RequesterId = "1",
            RequesterName = "Employee One",
            RequestType = "NewAsset",
            Status = RequestStatus.PendingStorekeeperReview
        };

        db.Users.Add(storekeeper);
        db.AssetRequests.Add(assetRequest);
        await db.SaveChangesAsync();

        var handler = new ProcessRequestCommandHandler(db);

        // Negative id => routes to the AssetRequests branch (unified requests list convention).
        await handler.Handle(new ProcessRequestCommand
        {
            Id = -assetRequest.Id,
            IsInStock = false,
            Remarks = "Out of stock, escalating to procurement",
            ProcessedByUserId = storekeeper.Id,
            CallerRole = Roles.Storekeeper
        }, CancellationToken.None);

        var saved = await db.AssetRequests.FindAsync(assetRequest.Id);

        Assert.NotNull(saved);
        Assert.Equal(storekeeper.Id, saved!.ProcessedByUserId);
        Assert.Equal("Store Keeper", saved.ProcessedByName);
        Assert.Equal("Out of stock, escalating to procurement", saved.ProcessorRemarks);
        Assert.NotNull(saved.ProcessedAt);
    }

    [Fact]
    public async Task GetFilteredAssetRequestsQuery_ReturnsStorekeeperInfoToRequestingEmployee()
    {
        using var db = TestContextFactory.CreateContext();

        var employee = new User
        {
            Id = 1,
            Username = "emp",
            FirstName = "Employee",
            LastName = "One",
            Email = "emp@example.com",
            PasswordHash = "x",
            Role = UserRole.Employee
        };

        var assetRequest = new AssetRequest
        {
            Id = 701,
            AssetName = "Office Chair",
            AssetCategory = "Furniture",
            Priority = "Low",
            RequesterId = "1",
            RequesterName = "Employee One",
            RequestType = "NewAsset",
            Status = RequestStatus.PendingProcurement,
            ProcessedByUserId = 900,
            ProcessedByName = "Store Keeper",
            ProcessorRemarks = "Not in stock, forwarded to procurement",
            ProcessedAt = DateTime.UtcNow
        };

        db.Users.Add(employee);
        db.AssetRequests.Add(assetRequest);
        await db.SaveChangesAsync();

        var handler = new GetFilteredAssetRequestsQueryHandler(db);
        var result = await handler.Handle(
            new GetFilteredAssetRequestsQuery(EmployeeId: "1"), CancellationToken.None);

        var dto = Assert.Single(result);
        Assert.Equal("Store Keeper", dto.ProcessedByName);
        Assert.Equal("Not in stock, forwarded to procurement", dto.ProcessorRemarks);
        Assert.NotNull(dto.ProcessedAt);
    }
}
