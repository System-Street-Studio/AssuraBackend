using Assura.Application.Features.Transfers.Commands;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Assura.Domain.Enums;

namespace Assura.Application.Tests;

// Covers the BUGS.md finding: CreateTransferCommand had no division-ownership check
// on the caller at all — every other command in the Transfer feature (Approve/
// Confirm/Cancel/Reject/ReturnActive) checks the caller's own division before
// acting, but CreateTransfer (the command that actually creates the row) never got
// that same treatment.
//
// The check is scoped to the *target* user's (requester's) division, not the
// current asset holder's — the Division Head who calls CreateTransfer from the
// Asset Pool is the one who approved the underlying Transfer-type AssetRequest
// (GetApprovedTransfersQueryHandler scopes that dropdown by
// AssetRequest.DivisionId == caller's own division, and AssetRequest.DivisionId is
// the requester's division), and is very often in a *different* division than the
// asset's current holder — that's the whole point of a transfer. An earlier version
// of this check scoped against the current holder's division instead, which broke
// every real cross-division transfer with a 500 (see BUGS.md for the incident).
// Admin is exempt, matching every other division-scoped command in this feature.
public class CreateTransferDivisionScopingTests
{
    private static async Task<Assura.Application.Tests.Common.TestApplicationDbContext> SeedAsync(int callerDivisionId, UserRole callerRole)
    {
        var db = TestContextFactory.CreateContext();

        db.Assets.Add(new Asset { Id = 1, AssetCode = "A1", Status = AssetStatus.InUse, AssignedUserId = 10 });
        db.Users.Add(new User { Id = 10, Role = UserRole.Employee, DivisionId = 5 }); // current holder, division 5
        db.Users.Add(new User { Id = 20, Role = UserRole.Employee, DivisionId = 9 }); // target user (requester), division 9
        db.Users.Add(new User { Id = 30, Role = callerRole, DivisionId = callerDivisionId }); // caller
        db.AssetRequests.Add(new AssetRequest
        {
            Id = 1,
            AssetName = "Laptop",
            Priority = "Normal",
            RequestType = "Transfer",
            RequesterId = "20",
            Reason = "Needed for project (Transfer periods: 1/1/2026 to 1/15/2026)"
        });
        await db.SaveChangesAsync();
        return db;
    }

    [Fact]
    public async Task Create_CallerInTargetUsersDivision_Succeeds()
    {
        using var db = await SeedAsync(callerDivisionId: 9, UserRole.DivisionHead);

        var handler = new CreateTransferCommandHandler(db);
        var id = await handler.Handle(new CreateTransferCommand { AssetId = 1, AssetRequestId = 1, UserId = 30 }, CancellationToken.None);

        Assert.True(id > 0);
        var transfer = await db.Transfers.FindAsync(id);
        Assert.NotNull(transfer);
        Assert.Equal(30, transfer!.TransferById);
    }

    // This is exactly the real-world case that a current-holder-scoped check broke:
    // the caller legitimately shares a division with the asset's current holder, but
    // not with the requester whose approved request they're fulfilling.
    [Fact]
    public async Task Create_CallerInCurrentHoldersDivisionButNotTargetUsers_ThrowsUnauthorized()
    {
        using var db = await SeedAsync(callerDivisionId: 5, UserRole.DivisionHead);

        var handler = new CreateTransferCommandHandler(db);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(new CreateTransferCommand { AssetId = 1, AssetRequestId = 1, UserId = 30 }, CancellationToken.None));

        Assert.Empty(db.Transfers);
    }

    [Fact]
    public async Task Create_CallerInUnrelatedDivision_ThrowsUnauthorized()
    {
        using var db = await SeedAsync(callerDivisionId: 42, UserRole.DivisionHead);

        var handler = new CreateTransferCommandHandler(db);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(new CreateTransferCommand { AssetId = 1, AssetRequestId = 1, UserId = 30 }, CancellationToken.None));

        Assert.Empty(db.Transfers);
    }

    [Fact]
    public async Task Create_ByAdmin_BypassesDivisionCheck()
    {
        using var db = await SeedAsync(callerDivisionId: 42, UserRole.Admin);

        var handler = new CreateTransferCommandHandler(db);
        var id = await handler.Handle(new CreateTransferCommand { AssetId = 1, AssetRequestId = 1, UserId = 30 }, CancellationToken.None);

        Assert.True(id > 0);
    }
}
