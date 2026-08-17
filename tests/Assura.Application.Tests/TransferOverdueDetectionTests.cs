using Assura.Application.Features.Transfers.Handlers;
using ConfirmHandler = Assura.Application.Features.Transfers.Handlers.ConfirmTransferByHeadCommandHandler;
using Assura.Application.Features.Transfers.Commands;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Assura.Domain.Enums;

namespace Assura.Application.Tests;

// Covers the newly-found bug: TransferOverdueCheckerService only ever flagged a
// transfer overdue when ReturnDate was set and in the past, but ReturnDate was only
// ever written by ReturnActiveTransferCommand at the moment a transfer *completes* —
// never when it becomes Active. So ReturnDate was always null on a real Active
// transfer, and the background job could never match a row. ConfirmTransferByHeadCommandHandler
// now populates a separate ExpectedReturnDate (parsed from TransferPeriod) when
// activating the transfer, which is what the overdue check should compare against.
public class TransferOverdueDetectionTests
{
    [Fact]
    public async Task Confirm_ParsesTransferPeriodEndDate_IntoExpectedReturnDate()
    {
        using var db = TestContextFactory.CreateContext();
        db.Transfers.Add(new Transfer
        {
            Id = 1,
            TransferNumber = "TRF-1",
            TransferDate = DateTime.UtcNow,
            AssetRequestId = 1,
            AssetId = 1,
            FromDivisionId = 5,
            ToDivisionId = 9,
            TargetUserId = 1,
            CurrentHolderId = 1,
            TransferPeriod = "1/1/2026 to 1/15/2026",
            Status = TransferStatus.WaitingForFinalConfirmation
        });
        db.Users.Add(new User { Id = 100, Role = UserRole.DivisionHead, DivisionId = 9 });
        await db.SaveChangesAsync();

        var handler = new ConfirmHandler(db);
        await handler.Handle(new ConfirmTransferByHeadCommand(1, UserId: 100), CancellationToken.None);

        var transfer = await db.Transfers.FindAsync(1);
        Assert.Equal(TransferStatus.Active, transfer!.Status);
        Assert.Equal(new DateTime(2026, 1, 15), transfer.ExpectedReturnDate!.Value.Date);
    }

    [Fact]
    public async Task Confirm_WithNoParsableTransferPeriod_LeavesExpectedReturnDateNull()
    {
        using var db = TestContextFactory.CreateContext();
        db.Transfers.Add(new Transfer
        {
            Id = 2,
            TransferNumber = "TRF-2",
            TransferDate = DateTime.UtcNow,
            AssetRequestId = 1,
            AssetId = 1,
            FromDivisionId = 5,
            ToDivisionId = 9,
            TargetUserId = 1,
            CurrentHolderId = 1,
            TransferPeriod = null,
            Status = TransferStatus.WaitingForFinalConfirmation
        });
        db.Users.Add(new User { Id = 101, Role = UserRole.DivisionHead, DivisionId = 9 });
        await db.SaveChangesAsync();

        var handler = new ConfirmHandler(db);
        await handler.Handle(new ConfirmTransferByHeadCommand(2, UserId: 101), CancellationToken.None);

        Assert.Null((await db.Transfers.FindAsync(2))!.ExpectedReturnDate);
    }
}
