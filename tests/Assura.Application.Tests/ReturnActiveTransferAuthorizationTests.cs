using Assura.Application.Features.Transfers.Commands;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Assura.Domain.Enums;

namespace Assura.Application.Tests;

// Covers the newly-found bug: POST /api/transfers/{id}/return had no ownership or
// role check at all — any authenticated user could mark any Active transfer as
// Completed and flip the asset back to InUse. The handler now requires the caller
// to be either the asset's new holder (TargetUserId) or the Division Head of either
// side of the transfer (matching the "active" tab scoping in
// GetDivisionHeadTransferQueryHandler, which shows the transfer to both heads).
public class ReturnActiveTransferAuthorizationTests
{
    private static Transfer MakeActiveTransfer(int id, int targetUserId, int fromDivisionId, int toDivisionId) => new()
    {
        Id = id,
        TransferNumber = $"TRF-{id}",
        TransferDate = DateTime.UtcNow,
        AssetRequestId = 1,
        AssetId = 1,
        FromDivisionId = fromDivisionId,
        ToDivisionId = toDivisionId,
        TargetUserId = targetUserId,
        CurrentHolderId = 999,
        Status = TransferStatus.Active
    };

    [Fact]
    public async Task Return_ByTargetUser_Succeeds()
    {
        using var db = TestContextFactory.CreateContext();
        db.Transfers.Add(MakeActiveTransfer(1, targetUserId: 50, fromDivisionId: 5, toDivisionId: 9));
        db.Assets.Add(new Asset { Id = 1, AssetCode = "A1", Status = AssetStatus.Transferred });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var handler = new ReturnActiveTransferCommandHandler(db);
        var result = await handler.Handle(new ReturnActiveTransferCommand(1, CallerId: 50, IsAdmin: false, IsDivisionHead: false), CancellationToken.None);

        Assert.True(result);
        Assert.Equal(TransferStatus.Completed, (await db.Transfers.FindAsync(1))!.Status);
    }

    [Fact]
    public async Task Return_ByDivisionHeadOfEitherSide_Succeeds()
    {
        using var db = TestContextFactory.CreateContext();
        db.Transfers.Add(MakeActiveTransfer(2, targetUserId: 50, fromDivisionId: 5, toDivisionId: 9));
        db.Assets.Add(new Asset { Id = 1, AssetCode = "A1", Status = AssetStatus.Transferred });
        db.Users.Add(new User { Id = 200, Role = UserRole.DivisionHead, DivisionId = 9 });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var handler = new ReturnActiveTransferCommandHandler(db);
        var result = await handler.Handle(new ReturnActiveTransferCommand(2, CallerId: 200, IsAdmin: false, IsDivisionHead: true), CancellationToken.None);

        Assert.True(result);
    }

    [Fact]
    public async Task Return_ByUnrelatedUser_ThrowsUnauthorized()
    {
        using var db = TestContextFactory.CreateContext();
        db.Transfers.Add(MakeActiveTransfer(3, targetUserId: 50, fromDivisionId: 5, toDivisionId: 9));
        db.Assets.Add(new Asset { Id = 1, AssetCode = "A1", Status = AssetStatus.Transferred });
        await db.SaveChangesAsync();

        var handler = new ReturnActiveTransferCommandHandler(db);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(new ReturnActiveTransferCommand(3, CallerId: 999, IsAdmin: false, IsDivisionHead: false), CancellationToken.None));

        Assert.Equal(TransferStatus.Active, (await db.Transfers.FindAsync(3))!.Status);
    }

    [Fact]
    public async Task Return_ByUnrelatedDivisionHead_ThrowsUnauthorized()
    {
        using var db = TestContextFactory.CreateContext();
        db.Transfers.Add(MakeActiveTransfer(4, targetUserId: 50, fromDivisionId: 5, toDivisionId: 9));
        db.Assets.Add(new Asset { Id = 1, AssetCode = "A1", Status = AssetStatus.Transferred });
        db.Users.Add(new User { Id = 201, Role = UserRole.DivisionHead, DivisionId = 42 });
        await db.SaveChangesAsync();

        var handler = new ReturnActiveTransferCommandHandler(db);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(new ReturnActiveTransferCommand(4, CallerId: 201, IsAdmin: false, IsDivisionHead: true), CancellationToken.None));
    }

    [Fact]
    public async Task Return_ByAdmin_Succeeds()
    {
        using var db = TestContextFactory.CreateContext();
        db.Transfers.Add(MakeActiveTransfer(5, targetUserId: 50, fromDivisionId: 5, toDivisionId: 9));
        db.Assets.Add(new Asset { Id = 1, AssetCode = "A1", Status = AssetStatus.Transferred });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var handler = new ReturnActiveTransferCommandHandler(db);
        var result = await handler.Handle(new ReturnActiveTransferCommand(5, CallerId: 1, IsAdmin: true, IsDivisionHead: false), CancellationToken.None);

        Assert.True(result);
    }
}
