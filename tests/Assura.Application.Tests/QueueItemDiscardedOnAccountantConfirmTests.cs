using Assura.Application.Common.Interfaces;
using Assura.Application.Features.AccPendingItems.Commands.ConfirmDiscard;
using Assura.Application.Features.QueueItems.Commands.UpdateStatus;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Assura.Application.Tests;

// Covers the BUGS.md Superintendent finding: "Overview UI can never produce a
// 'Discarded' queue item, even though the stat card and backend both expect one."
// QueueItemStatus.Discarded was never set by any code path. Per product clarification,
// "Discarded" means the asset has actually been confirmed disposed by the Accountant
// (ConfirmDiscardCommand), which is a separate, later step than the Superintendent's
// initial "Approved" decision. This test seeds a QueueItem, runs it through Approve
// (which now stamps AccPendingItem.QueueItemId) and then through the Accountant's
// discard confirmation, and asserts the QueueItem flips to Discarded only at that
// final step — it fails before the QueueItemId link existed.
public class QueueItemDiscardedOnAccountantConfirmTests
{
    [Fact]
    public async Task ConfirmDiscard_ShouldMarkOriginatingQueueItem_AsDiscarded()
    {
        using var db = TestContextFactory.CreateContext();

        var queueItem = new QueueItem { Name = "Old Printer", Division = "IT", AssetType = "Hardware", Status = QueueItemStatus.Pending };
        db.QueueItems.Add(queueItem);
        await db.SaveChangesAsync();

        var mockUserService = new Mock<ICurrentUserService>();
        mockUserService.Setup(m => m.UserId).Returns("1");
        var approveHandler = new UpdateQueueItemStatusCommandHandler(db, mockUserService.Object);
        await approveHandler.Handle(new UpdateQueueItemStatusCommand { Id = queueItem.Id, Status = "Approved" }, CancellationToken.None);

        var pendingItem = await db.AccPendingItems.SingleAsync();
        Assert.Equal(queueItem.Id, pendingItem.QueueItemId);

        // Still just "Approved" — the accountant hasn't confirmed the physical discard yet.
        var stillApproved = await db.QueueItems.FindAsync(queueItem.Id);
        Assert.Equal(QueueItemStatus.Approved, stillApproved!.Status);

        var confirmHandler = new ConfirmDiscardCommandHandler(db);
        var result = await confirmHandler.Handle(new ConfirmDiscardCommand { Id = pendingItem.Id }, CancellationToken.None);

        Assert.True(result);
        var discardedQueueItem = await db.QueueItems.FindAsync(queueItem.Id);
        Assert.Equal(QueueItemStatus.Discarded, discardedQueueItem!.Status);
    }

    // Covers the BUGS.md Accountant finding: "Discard queue detail panel never shows who
    // requested the discard." AccPendingItem.CurrentUser was populated with the approving
    // Superintendent's name, while the original requester (carried on QueueItem from
    // AssetRequestApprovedEventHandler) was silently dropped. This asserts the requester
    // identity survives from QueueItem through AccPendingItem into AccDiscardedItem.
    [Fact]
    public async Task RequesterIdentity_ShouldSurvive_FromQueueItemThroughToDiscardedItem()
    {
        using var db = TestContextFactory.CreateContext();

        var queueItem = new QueueItem
        {
            Name = "Old Printer",
            Division = "IT",
            AssetType = "Hardware",
            Status = QueueItemStatus.Pending,
            RequestedById = "42",
            RequestedByName = "Jane Employee"
        };
        db.QueueItems.Add(queueItem);
        await db.SaveChangesAsync();

        var mockUserService = new Mock<ICurrentUserService>();
        mockUserService.Setup(m => m.UserId).Returns("1");
        var approveHandler = new UpdateQueueItemStatusCommandHandler(db, mockUserService.Object);
        await approveHandler.Handle(new UpdateQueueItemStatusCommand { Id = queueItem.Id, Status = "Approved" }, CancellationToken.None);

        var pendingItem = await db.AccPendingItems.SingleAsync();
        Assert.Equal("Jane Employee", pendingItem.RequestedByName);
        Assert.Equal("42", pendingItem.RequestedById);
        // CurrentUser records the approver, distinct from the original requester.
        Assert.NotEqual(pendingItem.CurrentUser, pendingItem.RequestedByName);

        var confirmHandler = new ConfirmDiscardCommandHandler(db);
        await confirmHandler.Handle(new ConfirmDiscardCommand { Id = pendingItem.Id }, CancellationToken.None);

        var discardedItem = await db.AccDiscardedItems.SingleAsync();
        Assert.Equal("Jane Employee", discardedItem.RequestedByName);
    }
}
