using Assura.Application.Features.AssetRequests.Commands;
using Assura.Application.Features.AssetRequests.Queries;
using Assura.Application.Features.Notifications.Commands;
using Assura.Application.Features.Requests.Queries;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Assura.Domain.Enums;

namespace Assura.Application.Tests;

// Covers several BUGS.md Employee findings around missing ownership checks (IDOR) and
// trusted client-supplied identity, all previously letting one Employee read or spoof
// another user's data.
public class EmployeeIdorAndOwnershipTests
{
    [Fact]
    public async Task GetRequestByIdQuery_Employee_CannotViewAnotherUsersRequest()
    {
        using var db = TestContextFactory.CreateContext();

        var owner = new User { Id = 1, FirstName = "Owner", Role = UserRole.Employee };
        var attacker = new User { Id = 2, FirstName = "Attacker", Role = UserRole.Employee };
        db.Users.AddRange(owner, attacker);
        db.Requests.Add(new Request
        {
            Id = 10,
            RequestNumber = "REQ-10",
            RequesterId = owner.Id,
            Priority = PriorityType.Medium,
            Status = "Pending"
        });
        await db.SaveChangesAsync();

        var handler = new GetRequestByIdQueryHandler(db);

        var asAttacker = await handler.Handle(new GetRequestByIdQuery(10, attacker.Id, UserRole.Employee), CancellationToken.None);
        Assert.Null(asAttacker);

        var asOwner = await handler.Handle(new GetRequestByIdQuery(10, owner.Id, UserRole.Employee), CancellationToken.None);
        Assert.NotNull(asOwner);

        var asStorekeeper = await handler.Handle(new GetRequestByIdQuery(10, 999, UserRole.Storekeeper), CancellationToken.None);
        Assert.NotNull(asStorekeeper);
    }

    [Fact]
    public async Task GetAssetRequestByIdQuery_Employee_CannotViewAnotherUsersRequest()
    {
        using var db = TestContextFactory.CreateContext();

        db.AssetRequests.Add(new AssetRequest
        {
            Id = 20,
            AssetName = "Chair",
            Priority = "Normal",
            RequesterId = "1",
            RequesterName = "Owner",
            RequestType = "NewAsset",
            UserId = 1
        });
        await db.SaveChangesAsync();

        var handler = new GetAssetRequestByIdQueryHandler(db);

        var asAttacker = await handler.Handle(
            new GetAssetRequestByIdQuery { Id = 20, UserId = 2, Role = UserRole.Employee }, CancellationToken.None);
        Assert.Null(asAttacker);

        var asOwner = await handler.Handle(
            new GetAssetRequestByIdQuery { Id = 20, UserId = 1, Role = UserRole.Employee }, CancellationToken.None);
        Assert.NotNull(asOwner);

        var asAdmin = await handler.Handle(
            new GetAssetRequestByIdQuery { Id = 20, UserId = 999, Role = UserRole.Admin }, CancellationToken.None);
        Assert.NotNull(asAdmin);
    }

    // Covers a bug found by the test-workflow simulation: GetAssetRequestByIdQuery
    // treated DivisionHead as fully privileged like Admin/Procurement/Storekeeper, so a
    // Division Head from one division could read the full detail of another division's
    // asset request — confirmed live even though the write-side (approve/reject) was
    // already correctly division-scoped. This mirrors the same fix already applied to
    // the sibling Requests entity's GetRequestByIdQuery.
    [Fact]
    public async Task GetAssetRequestByIdQuery_DivisionHead_CannotViewAnotherDivisionsRequest()
    {
        using var db = TestContextFactory.CreateContext();

        var itHead = new User { Id = 3, FirstName = "IT", LastName = "Head", Role = UserRole.DivisionHead, DivisionId = 1 };
        var astroHead = new User { Id = 4, FirstName = "Astro", LastName = "Head", Role = UserRole.DivisionHead, DivisionId = 2 };
        db.Users.AddRange(itHead, astroHead);
        db.AssetRequests.Add(new AssetRequest
        {
            Id = 21,
            AssetName = "Monitor",
            Priority = "Normal",
            RequesterId = "1",
            RequesterName = "IT Employee",
            RequestType = "New Asset",
            UserId = 1,
            DivisionId = 1
        });
        await db.SaveChangesAsync();

        var handler = new GetAssetRequestByIdQueryHandler(db);

        var asWrongDivisionHead = await handler.Handle(
            new GetAssetRequestByIdQuery { Id = 21, UserId = astroHead.Id, Role = UserRole.DivisionHead }, CancellationToken.None);
        Assert.Null(asWrongDivisionHead);

        var asOwningDivisionHead = await handler.Handle(
            new GetAssetRequestByIdQuery { Id = 21, UserId = itHead.Id, Role = UserRole.DivisionHead }, CancellationToken.None);
        Assert.NotNull(asOwningDivisionHead);
    }

    [Fact]
    public async Task MarkNotificationAsRead_CannotMarkAnotherUsersNotification()
    {
        using var db = TestContextFactory.CreateContext();

        db.Notifications.Add(new Notification { Id = 30, UserId = 1, Title = "T", Message = "M" });
        await db.SaveChangesAsync();

        var handler = new MarkNotificationAsReadCommandHandler(db);

        await handler.Handle(new MarkNotificationAsReadCommand(30, 2), CancellationToken.None);
        var stillUnread = await db.Notifications.FindAsync(30);
        Assert.False(stillUnread!.IsRead);

        await handler.Handle(new MarkNotificationAsReadCommand(30, 1), CancellationToken.None);
        var nowRead = await db.Notifications.FindAsync(30);
        Assert.True(nowRead!.IsRead);
    }

    [Fact]
    public async Task CreateAssetRequestCommand_RequesterNameComesFromAuthenticatedUser_NotClientInput()
    {
        using var db = TestContextFactory.CreateContext();

        db.Users.Add(new User { Id = 1, FirstName = "Real", LastName = "Employee", Role = UserRole.Employee });
        await db.SaveChangesAsync();

        var handler = new CreateAssetRequestHandler(db);

        var id = await handler.Handle(new CreateAssetRequestCommand
        {
            EmployeeId = "1", // set by the controller from the JWT claim, not client input
            SubmittedBy = "Spoofed Name",
            AssetName = "Monitor",
            Priority = "Normal",
            RequestType = "NewAsset"
        }, CancellationToken.None);

        var saved = await db.AssetRequests.FindAsync(id);
        Assert.Equal("Real Employee", saved!.RequesterName);
        Assert.NotEqual("Spoofed Name", saved.RequesterName);
    }
}
