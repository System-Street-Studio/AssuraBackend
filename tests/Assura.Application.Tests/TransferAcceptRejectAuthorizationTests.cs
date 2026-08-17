using Assura.Application.Features.Transfers.Commands;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Assura.Domain.Enums;

namespace Assura.Application.Tests;

// Covers a bug found by the test-workflow simulation: AcceptTransferCommand and
// RejectTransferCommand (the plain, non-"-head" endpoints — the current holder's
// counterpart to accept/decline handing an asset over) took no caller identity at
// all and no status guard. Confirmed live: a completely unrelated Storekeeper
// account rejected an in-progress transfer between two other employees, and the
// *target* (not the current holder) successfully accepted a transfer on the
// current holder's behalf. Mirrors the same ownership-check pattern already applied
// to the four *-by-head handlers (TransferByHeadDivisionScopingTests.cs).
public class TransferAcceptRejectAuthorizationTests
{
    private static Transfer MakeTransfer(int id, int currentHolderId, int targetUserId, TransferStatus status) => new()
    {
        Id = id,
        TransferNumber = $"TRF-{id}",
        TransferDate = DateTime.UtcNow,
        AssetRequestId = 1,
        AssetId = 1,
        FromDivisionId = 5,
        ToDivisionId = 9,
        CurrentHolderId = currentHolderId,
        TargetUserId = targetUserId,
        Status = status
    };

    [Fact]
    public async Task Accept_ByCurrentHolder_Succeeds()
    {
        using var db = TestContextFactory.CreateContext();
        db.Transfers.Add(MakeTransfer(1, currentHolderId: 65, targetUserId: 110, TransferStatus.PendingOwnerApproval));
        await db.SaveChangesAsync();

        var handler = new AcceptTransferCommandHandler(db);
        var result = await handler.Handle(new AcceptTransferCommand(1, UserId: 65), CancellationToken.None);

        Assert.True(result);
        Assert.Equal(TransferStatus.PendingOwnerDivisionHeadApproval, (await db.Transfers.FindAsync(1))!.Status);
    }

    [Fact]
    public async Task Accept_ByTargetRecipient_ThrowsUnauthorized()
    {
        // Confirmed live: the *recipient* (not the current holder) could accept on
        // the holder's behalf before this fix.
        using var db = TestContextFactory.CreateContext();
        db.Transfers.Add(MakeTransfer(2, currentHolderId: 65, targetUserId: 110, TransferStatus.PendingOwnerApproval));
        await db.SaveChangesAsync();

        var handler = new AcceptTransferCommandHandler(db);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(new AcceptTransferCommand(2, UserId: 110), CancellationToken.None));

        Assert.Equal(TransferStatus.PendingOwnerApproval, (await db.Transfers.FindAsync(2))!.Status);
    }

    [Fact]
    public async Task Accept_ByUnrelatedUser_ThrowsUnauthorized()
    {
        // Confirmed live: a Storekeeper with no connection to the transfer at all.
        using var db = TestContextFactory.CreateContext();
        db.Transfers.Add(MakeTransfer(3, currentHolderId: 65, targetUserId: 110, TransferStatus.PendingOwnerApproval));
        await db.SaveChangesAsync();

        var handler = new AcceptTransferCommandHandler(db);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(new AcceptTransferCommand(3, UserId: 999), CancellationToken.None));
    }

    [Fact]
    public async Task Accept_WhenNotPendingOwnerApproval_ThrowsAndDoesNotChangeStatus()
    {
        // Confirmed live: accepting an already-accepted transfer silently "succeeded"
        // again with no status guard.
        using var db = TestContextFactory.CreateContext();
        db.Transfers.Add(MakeTransfer(4, currentHolderId: 65, targetUserId: 110, TransferStatus.PendingOwnerDivisionHeadApproval));
        await db.SaveChangesAsync();

        var handler = new AcceptTransferCommandHandler(db);

        await Assert.ThrowsAsync<Exception>(
            () => handler.Handle(new AcceptTransferCommand(4, UserId: 65), CancellationToken.None));

        Assert.Equal(TransferStatus.PendingOwnerDivisionHeadApproval, (await db.Transfers.FindAsync(4))!.Status);
    }

    [Fact]
    public async Task Reject_ByCurrentHolder_Succeeds()
    {
        using var db = TestContextFactory.CreateContext();
        db.Transfers.Add(MakeTransfer(5, currentHolderId: 65, targetUserId: 110, TransferStatus.PendingOwnerApproval));
        await db.SaveChangesAsync();

        var handler = new RejectTransferCommandHandler(db);
        var result = await handler.Handle(new RejectTransferCommand(5, UserId: 65), CancellationToken.None);

        Assert.True(result);
        Assert.Equal(TransferStatus.Rejected, (await db.Transfers.FindAsync(5))!.Status);
    }

    [Fact]
    public async Task Reject_ByUnrelatedUser_ThrowsUnauthorized()
    {
        // Confirmed live: an unrelated Storekeeper successfully rejected a transfer
        // between two other employees in two other divisions.
        using var db = TestContextFactory.CreateContext();
        db.Transfers.Add(MakeTransfer(6, currentHolderId: 65, targetUserId: 110, TransferStatus.PendingOwnerApproval));
        await db.SaveChangesAsync();

        var handler = new RejectTransferCommandHandler(db);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(new RejectTransferCommand(6, UserId: 999), CancellationToken.None));

        Assert.Equal(TransferStatus.PendingOwnerApproval, (await db.Transfers.FindAsync(6))!.Status);
    }

    [Fact]
    public async Task Reject_WhenAlreadyActive_ThrowsAndDoesNotChangeStatus()
    {
        using var db = TestContextFactory.CreateContext();
        db.Transfers.Add(MakeTransfer(7, currentHolderId: 65, targetUserId: 110, TransferStatus.Active));
        await db.SaveChangesAsync();

        var handler = new RejectTransferCommandHandler(db);

        await Assert.ThrowsAsync<Exception>(
            () => handler.Handle(new RejectTransferCommand(7, UserId: 65), CancellationToken.None));

        Assert.Equal(TransferStatus.Active, (await db.Transfers.FindAsync(7))!.Status);
    }
}
