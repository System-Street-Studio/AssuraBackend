using Assura.Application.Features.Transfers.Commands;
using Assura.Application.Features.Transfers.Handlers;
using ApproveHandler = Assura.Application.Features.Transfers.Handlers.ApproveTransferByHeadCommandHandler;
using ConfirmHandler = Assura.Application.Features.Transfers.Handlers.ConfirmTransferByHeadCommandHandler;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Assura.Domain.Enums;

namespace Assura.Application.Tests;

// Covers the BUGS.md Division Head finding: "IDOR: Any Division Head can
// approve/reject/confirm/cancel any other division's transfer." The four
// *-by-head handlers accepted a headId/UserId parameter but never checked it
// against the transfer's FromDivisionId/ToDivisionId, so any Division Head
// account could act on another division's transfer by guessing/enumerating
// transfer IDs. Each handler now looks up the caller's own division and
// compares it against whichever side of the transfer is currently awaiting
// action (matching the scoping GetDivisionHeadTransferQueryHandler already
// uses for its "incoming"/"outgoing"/"pending" tabs).
public class TransferByHeadDivisionScopingTests
{
    private static Transfer MakeTransfer(int id, int fromDivisionId, int toDivisionId, TransferStatus status) => new()
    {
        Id = id,
        TransferNumber = $"TRF-{id}",
        TransferDate = DateTime.UtcNow,
        AssetRequestId = 1,
        AssetId = 1,
        FromDivisionId = fromDivisionId,
        ToDivisionId = toDivisionId,
        TargetUserId = 1,
        CurrentHolderId = 1,
        Status = status
    };

    // --- Approve (awaiting the FROM division's head) ---

    [Fact]
    public async Task Approve_HeadOfFromDivision_Succeeds()
    {
        using var db = TestContextFactory.CreateContext();
        db.Transfers.Add(MakeTransfer(1, fromDivisionId: 5, toDivisionId: 9, TransferStatus.PendingOwnerDivisionHeadApproval));
        db.Users.Add(new User { Id = 100, Role = UserRole.DivisionHead, DivisionId = 5 });
        await db.SaveChangesAsync();

        var handler = new ApproveHandler(db);
        var result = await handler.Handle(new ApproveTransferByHeadCommand(1, UserId: 100), CancellationToken.None);

        Assert.True(result);
        Assert.Equal(TransferStatus.WaitingForFinalConfirmation, (await db.Transfers.FindAsync(1))!.Status);
    }

    [Fact]
    public async Task Approve_HeadOfUnrelatedDivision_ThrowsUnauthorized()
    {
        using var db = TestContextFactory.CreateContext();
        db.Transfers.Add(MakeTransfer(2, fromDivisionId: 5, toDivisionId: 9, TransferStatus.PendingOwnerDivisionHeadApproval));
        db.Users.Add(new User { Id = 101, Role = UserRole.DivisionHead, DivisionId = 42 });
        await db.SaveChangesAsync();

        var handler = new ApproveHandler(db);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(new ApproveTransferByHeadCommand(2, UserId: 101), CancellationToken.None));

        Assert.Equal(TransferStatus.PendingOwnerDivisionHeadApproval, (await db.Transfers.FindAsync(2))!.Status);
    }

    [Fact]
    public async Task Approve_HeadOfToDivision_ThrowsUnauthorized()
    {
        // The ToDivision head is a different stage's approver (Confirm), not Approve's.
        using var db = TestContextFactory.CreateContext();
        db.Transfers.Add(MakeTransfer(3, fromDivisionId: 5, toDivisionId: 9, TransferStatus.PendingOwnerDivisionHeadApproval));
        db.Users.Add(new User { Id = 102, Role = UserRole.DivisionHead, DivisionId = 9 });
        await db.SaveChangesAsync();

        var handler = new ApproveHandler(db);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(new ApproveTransferByHeadCommand(3, UserId: 102), CancellationToken.None));
    }

    // --- Confirm (awaiting the TO division's head) ---

    [Fact]
    public async Task Confirm_HeadOfToDivision_Succeeds()
    {
        using var db = TestContextFactory.CreateContext();
        db.Transfers.Add(MakeTransfer(4, fromDivisionId: 5, toDivisionId: 9, TransferStatus.WaitingForFinalConfirmation));
        db.Users.Add(new User { Id = 103, Role = UserRole.DivisionHead, DivisionId = 9 });
        await db.SaveChangesAsync();

        var handler = new ConfirmHandler(db);
        var result = await handler.Handle(new ConfirmTransferByHeadCommand(4, UserId: 103), CancellationToken.None);

        Assert.True(result);
        Assert.Equal(TransferStatus.Active, (await db.Transfers.FindAsync(4))!.Status);
    }

    [Fact]
    public async Task Confirm_HeadOfUnrelatedDivision_ThrowsUnauthorized()
    {
        using var db = TestContextFactory.CreateContext();
        db.Transfers.Add(MakeTransfer(5, fromDivisionId: 5, toDivisionId: 9, TransferStatus.WaitingForFinalConfirmation));
        db.Users.Add(new User { Id = 104, Role = UserRole.DivisionHead, DivisionId = 42 });
        await db.SaveChangesAsync();

        var handler = new ConfirmHandler(db);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(new ConfirmTransferByHeadCommand(5, UserId: 104), CancellationToken.None));
    }

    // --- Cancel (awaiting the TO division's head, PendingOwnerApproval stage) ---

    [Fact]
    public async Task Cancel_HeadOfToDivision_Succeeds()
    {
        using var db = TestContextFactory.CreateContext();
        db.Transfers.Add(MakeTransfer(6, fromDivisionId: 5, toDivisionId: 9, TransferStatus.PendingOwnerApproval));
        db.Users.Add(new User { Id = 105, Role = UserRole.DivisionHead, DivisionId = 9 });
        await db.SaveChangesAsync();

        var handler = new CancelTransferByHeadCommandHandler(db);
        var result = await handler.Handle(new CancelTransferByHeadCommand(6, UserId: 105), CancellationToken.None);

        Assert.True(result);
        Assert.Equal(TransferStatus.Cancelled, (await db.Transfers.FindAsync(6))!.Status);
    }

    [Fact]
    public async Task Cancel_HeadOfUnrelatedDivision_ThrowsUnauthorized()
    {
        using var db = TestContextFactory.CreateContext();
        db.Transfers.Add(MakeTransfer(7, fromDivisionId: 5, toDivisionId: 9, TransferStatus.PendingOwnerApproval));
        db.Users.Add(new User { Id = 106, Role = UserRole.DivisionHead, DivisionId = 42 });
        await db.SaveChangesAsync();

        var handler = new CancelTransferByHeadCommandHandler(db);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(new CancelTransferByHeadCommand(7, UserId: 106), CancellationToken.None));

        Assert.Equal(TransferStatus.PendingOwnerApproval, (await db.Transfers.FindAsync(7))!.Status);
    }

    // --- Reject (expected division depends on the current status) ---

    [Fact]
    public async Task Reject_AtPendingOwnerDivisionHeadApproval_RequiresFromDivisionHead()
    {
        using var db = TestContextFactory.CreateContext();
        db.Transfers.Add(MakeTransfer(8, fromDivisionId: 5, toDivisionId: 9, TransferStatus.PendingOwnerDivisionHeadApproval));
        db.Users.Add(new User { Id = 107, Role = UserRole.DivisionHead, DivisionId = 5 });
        await db.SaveChangesAsync();

        var handler = new RejectTransferByHeadCommandHandler(db);
        var result = await handler.Handle(new RejectTransferByHeadCommand(8, DivisionHeadId: 107, Reason: "no"), CancellationToken.None);

        Assert.True(result);
        Assert.Equal(TransferStatus.RejectedByDivisionHead, (await db.Transfers.FindAsync(8))!.Status);
    }

    [Fact]
    public async Task Reject_AtWaitingForFinalConfirmation_RequiresToDivisionHead()
    {
        using var db = TestContextFactory.CreateContext();
        db.Transfers.Add(MakeTransfer(9, fromDivisionId: 5, toDivisionId: 9, TransferStatus.WaitingForFinalConfirmation));
        db.Users.Add(new User { Id = 108, Role = UserRole.DivisionHead, DivisionId = 9 });
        await db.SaveChangesAsync();

        var handler = new RejectTransferByHeadCommandHandler(db);
        var result = await handler.Handle(new RejectTransferByHeadCommand(9, DivisionHeadId: 108, Reason: "no"), CancellationToken.None);

        Assert.True(result);
    }

    [Fact]
    public async Task Reject_ByUnrelatedDivisionHead_ThrowsUnauthorized()
    {
        using var db = TestContextFactory.CreateContext();
        db.Transfers.Add(MakeTransfer(10, fromDivisionId: 5, toDivisionId: 9, TransferStatus.PendingOwnerDivisionHeadApproval));
        db.Users.Add(new User { Id = 109, Role = UserRole.DivisionHead, DivisionId = 42 });
        await db.SaveChangesAsync();

        var handler = new RejectTransferByHeadCommandHandler(db);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(new RejectTransferByHeadCommand(10, DivisionHeadId: 109, Reason: "no"), CancellationToken.None));

        Assert.Equal(TransferStatus.PendingOwnerDivisionHeadApproval, (await db.Transfers.FindAsync(10))!.Status);
    }
}
