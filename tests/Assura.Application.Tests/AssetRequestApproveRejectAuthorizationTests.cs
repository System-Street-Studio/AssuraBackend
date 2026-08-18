using Assura.Application.Features.AssetRequests.Commands;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using MediatR;
using Moq;

namespace Assura.Application.Tests;

// Covers the BUGS.md Division Head finding: "Missing role restriction on Asset Request
// approve/reject — any authenticated user (including Employee) can approve or reject any
// asset request." The controller previously had no [Authorize(Roles=...)] on these
// endpoints, and the handlers took only an Id with no caller context or division check,
// so a Division Head (or anyone) could approve/reject a request from another division,
// or re-decide a request that had already been approved/rejected. This adds:
//   - [Authorize(Roles = "DivisionHead,Admin")] on the controller actions
//   - a division-ownership check in the handlers (Admin bypasses it)
//   - a status guard so an already-decided request can't be re-approved/rejected
public class AssetRequestApproveRejectAuthorizationTests
{
    private static AssetRequest MakeRequest(int id, int? divisionId, RequestStatus status = RequestStatus.Pending) => new()
    {
        Id = id,
        AssetName = "Laptop",
        Priority = "Normal",
        RequesterId = "10",
        RequesterName = "Employee One",
        RequestType = "NewAsset",
        Status = status,
        DivisionId = divisionId
    };

    [Fact]
    public async Task Approve_DivisionHeadOfSameDivision_Succeeds()
    {
        using var db = TestContextFactory.CreateContext();
        db.AssetRequests.Add(MakeRequest(1, divisionId: 5));
        db.Users.Add(new User { Id = 100, Role = UserRole.DivisionHead, DivisionId = 5 });
        await db.SaveChangesAsync();

        var handler = new ApproveAssetRequestHandler(db, Mock.Of<IPublisher>());
        var result = await handler.Handle(new ApproveAssetRequestCommand(1, UserId: 100, IsAdmin: false), CancellationToken.None);

        Assert.Equal(ApproveAssetRequestResult.Success, result);
        Assert.Equal(RequestStatus.Approved, (await db.AssetRequests.FindAsync(1))!.Status);
    }

    [Fact]
    public async Task Approve_DivisionHeadOfDifferentDivision_ReturnsForbidden()
    {
        using var db = TestContextFactory.CreateContext();
        db.AssetRequests.Add(MakeRequest(2, divisionId: 5));
        db.Users.Add(new User { Id = 101, Role = UserRole.DivisionHead, DivisionId = 9 });
        await db.SaveChangesAsync();

        var handler = new ApproveAssetRequestHandler(db, Mock.Of<IPublisher>());
        var result = await handler.Handle(new ApproveAssetRequestCommand(2, UserId: 101, IsAdmin: false), CancellationToken.None);

        Assert.Equal(ApproveAssetRequestResult.Forbidden, result);
        Assert.Equal(RequestStatus.Pending, (await db.AssetRequests.FindAsync(2))!.Status);
    }

    [Fact]
    public async Task Approve_Admin_BypassesDivisionCheck()
    {
        using var db = TestContextFactory.CreateContext();
        db.AssetRequests.Add(MakeRequest(3, divisionId: 5));
        db.Users.Add(new User { Id = 102, Role = UserRole.Admin, DivisionId = null });
        await db.SaveChangesAsync();

        var handler = new ApproveAssetRequestHandler(db, Mock.Of<IPublisher>());
        var result = await handler.Handle(new ApproveAssetRequestCommand(3, UserId: 102, IsAdmin: true), CancellationToken.None);

        Assert.Equal(ApproveAssetRequestResult.Success, result);
    }

    [Fact]
    public async Task Approve_AlreadyDecidedRequest_ReturnsInvalidStatus()
    {
        using var db = TestContextFactory.CreateContext();
        db.AssetRequests.Add(MakeRequest(4, divisionId: 5, status: RequestStatus.Rejected));
        db.Users.Add(new User { Id = 100, Role = UserRole.DivisionHead, DivisionId = 5 });
        await db.SaveChangesAsync();

        var handler = new ApproveAssetRequestHandler(db, Mock.Of<IPublisher>());
        var result = await handler.Handle(new ApproveAssetRequestCommand(4, UserId: 100, IsAdmin: false), CancellationToken.None);

        Assert.Equal(ApproveAssetRequestResult.InvalidStatus, result);
    }

    [Fact]
    public async Task Approve_UnknownId_ReturnsNotFound()
    {
        using var db = TestContextFactory.CreateContext();

        var handler = new ApproveAssetRequestHandler(db, Mock.Of<IPublisher>());
        var result = await handler.Handle(new ApproveAssetRequestCommand(999, UserId: 100, IsAdmin: false), CancellationToken.None);

        Assert.Equal(ApproveAssetRequestResult.NotFound, result);
    }

    [Fact]
    public async Task Reject_DivisionHeadOfDifferentDivision_ReturnsForbidden()
    {
        using var db = TestContextFactory.CreateContext();
        db.AssetRequests.Add(MakeRequest(5, divisionId: 5));
        db.Users.Add(new User { Id = 101, Role = UserRole.DivisionHead, DivisionId = 9 });
        await db.SaveChangesAsync();

        var handler = new RejectAssetRequestHandler(db);
        var result = await handler.Handle(new RejectAssetRequestCommand(5, UserId: 101, IsAdmin: false), CancellationToken.None);

        Assert.Equal(RejectAssetRequestResult.Forbidden, result);
        Assert.Equal(RequestStatus.Pending, (await db.AssetRequests.FindAsync(5))!.Status);
    }

    [Fact]
    public async Task Reject_DivisionHeadOfSameDivision_Succeeds()
    {
        using var db = TestContextFactory.CreateContext();
        db.AssetRequests.Add(MakeRequest(6, divisionId: 5));
        db.Users.Add(new User { Id = 100, Role = UserRole.DivisionHead, DivisionId = 5 });
        await db.SaveChangesAsync();

        var handler = new RejectAssetRequestHandler(db);
        var result = await handler.Handle(new RejectAssetRequestCommand(6, UserId: 100, IsAdmin: false), CancellationToken.None);

        Assert.Equal(RejectAssetRequestResult.Success, result);
        Assert.Equal(RequestStatus.Rejected, (await db.AssetRequests.FindAsync(6))!.Status);
    }

    [Fact]
    public async Task Reject_AlreadyDecidedRequest_ReturnsInvalidStatus()
    {
        using var db = TestContextFactory.CreateContext();
        db.AssetRequests.Add(MakeRequest(7, divisionId: 5, status: RequestStatus.Approved));
        db.Users.Add(new User { Id = 100, Role = UserRole.DivisionHead, DivisionId = 5 });
        await db.SaveChangesAsync();

        var handler = new RejectAssetRequestHandler(db);
        var result = await handler.Handle(new RejectAssetRequestCommand(7, UserId: 100, IsAdmin: false), CancellationToken.None);

        Assert.Equal(RejectAssetRequestResult.InvalidStatus, result);
    }
}
