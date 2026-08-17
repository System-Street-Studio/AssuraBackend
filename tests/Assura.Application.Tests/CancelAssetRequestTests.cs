using Assura.Application.Features.AssetRequests.Commands;
using Assura.Application.Features.AssetRequests.Queries;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Assura.Domain.Enums;

namespace Assura.Application.Tests;

// Covers the API-contract audit finding: the Employee "Cancel request" button (fixed in
// a prior pass to actually call the backend) posts to POST /api/AssetRequests/{id}/cancel,
// but that route never existed server-side — every cancel attempt 404'd. This adds the
// missing command/handler/endpoint and verifies ownership + status-transition rules.
public class CancelAssetRequestTests
{
    [Fact]
    public async Task Handle_OwnerCancelsPendingRequest_SetsStatusToCancelled()
    {
        using var db = TestContextFactory.CreateContext();
        db.AssetRequests.Add(new AssetRequest
        {
            Id = 1,
            AssetName = "Laptop",
            Priority = "Normal",
            RequesterId = "10",
            RequesterName = "Employee One",
            RequestType = "NewAsset",
            Status = RequestStatus.Pending
        });
        await db.SaveChangesAsync();

        var handler = new CancelAssetRequestHandler(db);
        var result = await handler.Handle(new CancelAssetRequestCommand(1, UserId: 10, IsPrivileged: false), CancellationToken.None);

        Assert.Equal(CancelAssetRequestResult.Success, result);
        var saved = await db.AssetRequests.FindAsync(1);
        Assert.Equal(RequestStatus.Cancelled, saved!.Status);
    }

    [Fact]
    public async Task Handle_NonOwnerNonPrivilegedCaller_ReturnsForbidden()
    {
        using var db = TestContextFactory.CreateContext();
        db.AssetRequests.Add(new AssetRequest
        {
            Id = 2,
            AssetName = "Laptop",
            Priority = "Normal",
            RequesterId = "10",
            RequesterName = "Employee One",
            RequestType = "NewAsset",
            Status = RequestStatus.Pending
        });
        await db.SaveChangesAsync();

        var handler = new CancelAssetRequestHandler(db);
        var result = await handler.Handle(new CancelAssetRequestCommand(2, UserId: 999, IsPrivileged: false), CancellationToken.None);

        Assert.Equal(CancelAssetRequestResult.Forbidden, result);
        var saved = await db.AssetRequests.FindAsync(2);
        Assert.Equal(RequestStatus.Pending, saved!.Status);
    }

    [Fact]
    public async Task Handle_PrivilegedCallerCancelsOnBehalfOfEmployee_Succeeds()
    {
        using var db = TestContextFactory.CreateContext();
        db.AssetRequests.Add(new AssetRequest
        {
            Id = 3,
            AssetName = "Laptop",
            Priority = "Normal",
            RequesterId = "10",
            RequesterName = "Employee One",
            RequestType = "NewAsset",
            Status = RequestStatus.Pending
        });
        await db.SaveChangesAsync();

        var handler = new CancelAssetRequestHandler(db);
        var result = await handler.Handle(new CancelAssetRequestCommand(3, UserId: 999, IsPrivileged: true), CancellationToken.None);

        Assert.Equal(CancelAssetRequestResult.Success, result);
    }

    [Fact]
    public async Task Handle_RequestAlreadyProcessed_ReturnsInvalidStatus()
    {
        using var db = TestContextFactory.CreateContext();
        db.AssetRequests.Add(new AssetRequest
        {
            Id = 4,
            AssetName = "Laptop",
            Priority = "Normal",
            RequesterId = "10",
            RequesterName = "Employee One",
            RequestType = "NewAsset",
            Status = RequestStatus.Approved
        });
        await db.SaveChangesAsync();

        var handler = new CancelAssetRequestHandler(db);
        var result = await handler.Handle(new CancelAssetRequestCommand(4, UserId: 10, IsPrivileged: false), CancellationToken.None);

        Assert.Equal(CancelAssetRequestResult.InvalidStatus, result);
        var saved = await db.AssetRequests.FindAsync(4);
        Assert.Equal(RequestStatus.Approved, saved!.Status);
    }

    [Fact]
    public async Task Handle_UnknownId_ReturnsNotFound()
    {
        using var db = TestContextFactory.CreateContext();

        var handler = new CancelAssetRequestHandler(db);
        var result = await handler.Handle(new CancelAssetRequestCommand(999, UserId: 10, IsPrivileged: false), CancellationToken.None);

        Assert.Equal(CancelAssetRequestResult.NotFound, result);
    }

    // Covers the second contract-audit finding: GetPendingRequestsQuery had a separate
    // projection from GetFilteredAssetRequestsQuery that omitted ProcessedByName/
    // ProcessorRemarks/ProcessedAt, so those fields were always null on the "pending"
    // endpoint even when set on the entity.
    [Fact]
    public async Task GetPendingRequestsQuery_IncludesProcessorFields()
    {
        using var db = TestContextFactory.CreateContext();
        db.AssetRequests.Add(new AssetRequest
        {
            Id = 5,
            AssetName = "Laptop",
            Priority = "Normal",
            RequesterId = "10",
            RequesterName = "Employee One",
            RequestType = "NewAsset",
            Status = RequestStatus.Pending,
            ProcessedByName = "Store Keeper",
            ProcessorRemarks = "Checking stock",
            ProcessedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new GetPendingRequestsQueryHandler(db);
        var result = await handler.Handle(new GetPendingRequestsQuery(EmployeeId: "10"), CancellationToken.None);

        var dto = Assert.Single(result);
        Assert.Equal("Store Keeper", dto.ProcessedByName);
        Assert.Equal("Checking stock", dto.ProcessorRemarks);
        Assert.NotNull(dto.ProcessedAt);
    }
}
