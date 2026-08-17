using Assura.Application.Features.AssetRequests.Queries;
using Assura.Application.Features.Requests.Queries;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Assura.Domain.Enums;

namespace Assura.Application.Tests;

// Covers the new "show the assignee of the asset to be discarded" requirement
// requested alongside the WORKFLOW_BASELINE_discarding.md fixes: the Division Head
// reviewing a discard request had no way to see who the asset was assigned to. Both
// query handlers here back the discard-details page (list navigation vs. direct/refresh
// load), so both need the same AssigneeName resolution.
public class AssetRequestAssigneeDisplayTests
{
    [Fact]
    public async Task GetFilteredAssetRequestsQuery_ShouldReturnAssigneeName_FromLinkedAsset()
    {
        using var db = TestContextFactory.CreateContext();

        var assignee = new User { FirstName = "IT", LastName = "Employee", Role = UserRole.Employee };
        db.Users.Add(assignee);
        var asset = new Asset { AssetCode = "AST-0100", Status = AssetStatus.InUse };
        db.Assets.Add(asset);
        await db.SaveChangesAsync();

        asset.AssignedUserId = assignee.Id;
        db.AssetRequests.Add(new AssetRequest
        {
            AssetName = "Laptop (AST-0100)",
            Priority = "Normal",
            RequesterId = "1",
            RequesterName = "IT Employee",
            RequestType = "Discard",
            AssetId = asset.Id
        });
        await db.SaveChangesAsync();

        var handler = new GetFilteredAssetRequestsQueryHandler(db);
        var result = await handler.Handle(new GetFilteredAssetRequestsQuery(), CancellationToken.None);

        var dto = Assert.Single(result);
        Assert.Equal("IT Employee", dto.AssigneeName);
    }

    [Fact]
    public async Task GetRequestByIdQuery_ForAssetRequest_ShouldReturnAssigneeName_FromLinkedAsset()
    {
        using var db = TestContextFactory.CreateContext();

        var assignee = new User { FirstName = "IT", LastName = "Employee", Role = UserRole.Employee };
        db.Users.Add(assignee);
        var asset = new Asset { AssetCode = "AST-0100", Status = AssetStatus.InUse };
        db.Assets.Add(asset);
        await db.SaveChangesAsync();

        asset.AssignedUserId = assignee.Id;
        var ar = new AssetRequest
        {
            AssetName = "Laptop (AST-0100)",
            Priority = "Normal",
            RequesterId = "1",
            RequesterName = "IT Employee",
            RequestType = "Discard",
            UserId = 1,
            AssetId = asset.Id
        };
        db.AssetRequests.Add(ar);
        await db.SaveChangesAsync();

        var handler = new GetRequestByIdQueryHandler(db);
        var result = await handler.Handle(new GetRequestByIdQuery(-ar.Id, 1, UserRole.Employee), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("IT Employee", result!.AssigneeName);
    }
}
