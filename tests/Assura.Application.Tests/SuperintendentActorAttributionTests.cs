using Assura.Application.Common.Interfaces;
using Assura.Application.Features.QueueItems.Commands.UpdateStatus;
using Assura.Application.Features.DiscardedNotes.Commands.UpdateStatus;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using Assura.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Assura.Application.Tests;

// Covers the BUGS.md Superintendent finding: "Approve/discard actions are always
// attributed to 'Superintendent,' even when an Admin performed them." Both handlers
// hardcoded CurrentUser = "Superintendent" and the notification text regardless of
// which authorized role (Superintendent or Admin) actually acted. This test seeds a
// User with Id 1, mocks ICurrentUserService.UserId to "1" (shared with both the
// DbContext and the handler), and asserts the resolved actor's real name is used
// instead of the literal string.
public class SuperintendentActorAttributionTests
{
    private static (TestApplicationDbContext db, ICurrentUserService currentUserService) CreateContextWithSharedUserService()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var mockUserService = new Mock<ICurrentUserService>();
        mockUserService.Setup(m => m.UserId).Returns("1");

        return (new TestApplicationDbContext(options, mockUserService.Object), mockUserService.Object);
    }

    [Fact]
    public async Task UpdateQueueItemStatus_ShouldAttributeAccPendingItemToActingUser_NotHardcodedSuperintendent()
    {
        var (db, currentUserService) = CreateContextWithSharedUserService();
        using var _ = db;

        db.Users.Add(new User { Id = 1, FirstName = "Alice", LastName = "Admin", Role = UserRole.Admin });
        var queueItem = new QueueItem { Name = "Old Printer", Division = "IT", AssetType = "Hardware", Status = QueueItemStatus.Pending };
        db.QueueItems.Add(queueItem);
        await db.SaveChangesAsync();

        var handler = new UpdateQueueItemStatusCommandHandler(db, currentUserService);
        var command = new UpdateQueueItemStatusCommand { Id = queueItem.Id, Status = "Approved" };

        await handler.Handle(command, CancellationToken.None);

        var pendingItem = Assert.Single(await db.AccPendingItems.ToListAsync());
        Assert.Equal("Alice Admin", pendingItem.CurrentUser);
        Assert.NotEqual("Superintendent", pendingItem.CurrentUser);

        var notification = Assert.Single(await db.Notifications.ToListAsync());
        Assert.Contains("Alice Admin", notification.Message);
        Assert.DoesNotContain("Superintendent", notification.Message);
    }

    [Fact]
    public async Task UpdateDiscardedNoteStatus_ShouldAttributeAccPendingItemToActingUser_NotHardcodedSuperintendent()
    {
        var (db, currentUserService) = CreateContextWithSharedUserService();
        using var _ = db;

        db.Users.Add(new User { Id = 1, FirstName = "Bob", LastName = "Superintendent", Role = UserRole.Superintendent });
        db.Users.Add(new User { Id = 2, FirstName = "Carla", LastName = "Accountant", Role = UserRole.Accountant });
        var note = new DiscardedNote { Name = "Broken Chair", Division = "Facilities", AssetType = "Furniture", Status = DiscardNoteStatus.Pending };
        db.DiscardedNotes.Add(note);
        await db.SaveChangesAsync();

        var handler = new UpdateDiscardedNoteStatusCommandHandler(db, currentUserService);
        var command = new UpdateDiscardedNoteStatusCommand { Id = note.Id, Status = "Completed" };

        await handler.Handle(command, CancellationToken.None);

        var pendingItem = Assert.Single(await db.AccPendingItems.ToListAsync());
        Assert.Equal("Bob Superintendent", pendingItem.CurrentUser);

        var notification = Assert.Single(await db.Notifications.ToListAsync());
        Assert.Contains("Bob Superintendent", notification.Message);
    }
}
