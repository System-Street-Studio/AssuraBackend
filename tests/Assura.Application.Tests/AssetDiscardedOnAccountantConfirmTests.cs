using Assura.Application.Common.Interfaces;
using Assura.Application.Features.AccPendingItems.Commands.ConfirmDiscard;
using Assura.Application.Features.DiscardedNotes.Commands.UpdateStatus;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Assura.Application.Tests;

// Covers the /verify-workflow finding in WORKFLOW_BASELINE_discarding.md: running an
// Employee-initiated discard request all the way through Division Head approval,
// Superintendent completion, and Accountant confirmation left the real Asset record
// untouched (still "InUse", still assigned to its original user) because none of
// DiscardedNote/AccPendingItem carried the source AssetId forward for
// ConfirmDiscardCommand to act on. This asserts the Asset now flips to Discarded and
// is unassigned once the accountant confirms — it fails without the AssetId threading
// added to AssetRequestApprovedEventHandler / UpdateDiscardedNoteStatusCommandHandler /
// ConfirmDiscardCommandHandler. Also covers the QueueItemId and requester-identity
// gaps found in the same baseline: the Superintendent Overview dashboard's QueueItem
// never flipped to Discarded for this entry point, and the requester name dropped to
// null on the AccPendingItem, both because UpdateDiscardedNoteStatusCommandHandler
// never carried those fields from the DiscardedNote onto the AccPendingItem it creates.
public class AssetDiscardedOnAccountantConfirmTests
{
    [Fact]
    public async Task ConfirmDiscard_ShouldMarkOriginatingAsset_AsDiscarded_AndUnassignIt()
    {
        using var db = TestContextFactory.CreateContext();

        var asset = new Asset
        {
            AssetCode = "AST-TEST-01",
            AssetDate = DateTime.UtcNow,
            Status = AssetStatus.InUse,
            PurchaseValue = 100,
            AssignedUserId = 65
        };
        db.Assets.Add(asset);

        // Mirrors AssetRequestApprovedEventHandler's Discard branch: a matching
        // QueueItem is created alongside the DiscardedNote for the Superintendent
        // Overview dashboard.
        var queueItem = new QueueItem { Name = "Old Printer", Division = "IT", AssetType = "Hardware", Status = QueueItemStatus.Pending };
        db.QueueItems.Add(queueItem);
        await db.SaveChangesAsync();

        var discardedNote = new DiscardedNote
        {
            Name = "Old Printer",
            Division = "IT",
            Status = DiscardNoteStatus.Pending,
            AssetType = "Hardware",
            AssetId = asset.Id,
            QueueItemId = queueItem.Id,
            RequestedByUserId = 65,
            RequestedByName = "IT Employee"
        };
        db.DiscardedNotes.Add(discardedNote);
        await db.SaveChangesAsync();

        var mockUserService = new Mock<ICurrentUserService>();
        mockUserService.Setup(m => m.UserId).Returns("1");
        var completeHandler = new UpdateDiscardedNoteStatusCommandHandler(db, mockUserService.Object);
        await completeHandler.Handle(
            new UpdateDiscardedNoteStatusCommand { Id = discardedNote.Id, Status = "Completed", Note = "Verified disposed" },
            CancellationToken.None);

        var pendingItem = await db.AccPendingItems.SingleAsync();
        Assert.Equal(asset.Id, pendingItem.AssetId);
        Assert.Equal(queueItem.Id, pendingItem.QueueItemId);
        Assert.Equal("65", pendingItem.RequestedById);
        Assert.Equal("IT Employee", pendingItem.RequestedByName);

        // Still untouched — the accountant hasn't confirmed the physical discard yet.
        var stillInUse = await db.Assets.FindAsync(asset.Id);
        Assert.Equal(AssetStatus.InUse, stillInUse!.Status);
        var stillPendingQueueItem = await db.QueueItems.FindAsync(queueItem.Id);
        Assert.Equal(QueueItemStatus.Pending, stillPendingQueueItem!.Status);

        var receipt = new Receipt { AssetName = "Old Printer", Division = "IT", Amount = 50, FileUrl = "/uploads/receipts/r1.pdf" };
        db.Receipts.Add(receipt);
        await db.SaveChangesAsync();

        var confirmHandler = new ConfirmDiscardCommandHandler(db);
        var result = await confirmHandler.Handle(new ConfirmDiscardCommand { Id = pendingItem.Id, ReceiptId = receipt.Id }, CancellationToken.None);

        Assert.True(result);
        var discardedAsset = await db.Assets.FindAsync(asset.Id);
        Assert.Equal(AssetStatus.Discarded, discardedAsset!.Status);
        Assert.Null(discardedAsset.AssignedUserId);

        // The Superintendent Overview dashboard's QueueItem should now also flip to
        // Discarded, now that AccPendingItem.QueueItemId carries the link through.
        var discardedQueueItem = await db.QueueItems.FindAsync(queueItem.Id);
        Assert.Equal(QueueItemStatus.Discarded, discardedQueueItem!.Status);
    }
}
