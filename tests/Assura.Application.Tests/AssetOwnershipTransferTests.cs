using Assura.Application.Features.Transfers.Commands;
using Assura.Application.Features.Transfers.Handlers;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Assura.Domain.Enums;

namespace Assura.Application.Tests;

// Covers the BUGS.md finding: asset ownership never actually transferred —
// Asset.AssignedUserId/Status were never written by the Transfer workflow, so the
// Transfer row's status could correctly show Active/Completed while the asset itself
// stayed permanently attributed to its original holder in every other view.
public class AssetOwnershipTransferTests
{
    [Fact]
    public async Task Confirm_MovesAssetToTargetUser_AndMarksTransferred()
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
            TargetUserId = 77,
            CurrentHolderId = 55,
            Status = TransferStatus.WaitingForFinalConfirmation
        });
        db.Assets.Add(new Asset { Id = 1, AssetCode = "A1", Status = AssetStatus.InUse, AssignedUserId = 55 });
        db.Users.Add(new User { Id = 100, Role = UserRole.DivisionHead, DivisionId = 9 });
        await db.SaveChangesAsync();

        var handler = new ConfirmTransferByHeadCommandHandler(db);
        var result = await handler.Handle(new ConfirmTransferByHeadCommand(1, UserId: 100), CancellationToken.None);

        Assert.True(result);
        var asset = await db.Assets.FindAsync(1);
        Assert.Equal(77, asset!.AssignedUserId);
        Assert.Equal(AssetStatus.Transferred, asset.Status);
    }

    [Fact]
    public async Task Return_MovesAssetBackToOriginalHolder_AndMarksInUse()
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
            TargetUserId = 77,
            CurrentHolderId = 55,
            Status = TransferStatus.Active
        });
        db.Assets.Add(new Asset { Id = 1, AssetCode = "A1", Status = AssetStatus.Transferred, AssignedUserId = 77 });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var handler = new ReturnActiveTransferCommandHandler(db);
        var result = await handler.Handle(
            new ReturnActiveTransferCommand(2, CallerId: 77, IsAdmin: false, IsDivisionHead: false),
            CancellationToken.None);

        Assert.True(result);
        var asset = await db.Assets.FindAsync(1);
        Assert.Equal(55, asset!.AssignedUserId);
        Assert.Equal(AssetStatus.InUse, asset.Status);
    }
}
