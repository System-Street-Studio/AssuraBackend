using Assura.Application.Features.Transfers.Commands;
using Assura.Application.Features.Transfers.Queries;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Assura.Domain.Enums;

namespace Assura.Application.Tests;

// Covers the BUGS.md finding: TransferStatus.Overdue was a dead end — once
// TransferOverdueCheckerService flipped a transfer from Active to Overdue, it could
// no longer be returned (ReturnActiveTransferCommandHandler's status guard only
// accepted Active), and it disappeared from every "active" tab/count in both the
// Employee and Division Head query handlers.
public class TransferOverdueVisibilityTests
{
    private static Transfer MakeOverdueTransfer(int id, int targetUserId, int currentHolderId, int fromDivisionId, int toDivisionId) => new()
    {
        Id = id,
        TransferNumber = $"TRF-{id}",
        TransferDate = DateTime.UtcNow,
        AssetRequestId = 1,
        AssetId = 1,
        FromDivisionId = fromDivisionId,
        ToDivisionId = toDivisionId,
        TargetUserId = targetUserId,
        CurrentHolderId = currentHolderId,
        Status = TransferStatus.Overdue
    };

    [Fact]
    public async Task Return_FromOverdueStatus_Succeeds()
    {
        using var db = TestContextFactory.CreateContext();
        db.Transfers.Add(MakeOverdueTransfer(1, targetUserId: 50, currentHolderId: 55, fromDivisionId: 5, toDivisionId: 9));
        db.Assets.Add(new Asset { Id = 1, AssetCode = "A1", Status = AssetStatus.Transferred, AssignedUserId = 50 });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var handler = new ReturnActiveTransferCommandHandler(db);
        var result = await handler.Handle(
            new ReturnActiveTransferCommand(1, CallerId: 50, IsAdmin: false, IsDivisionHead: false),
            CancellationToken.None);

        Assert.True(result);
        Assert.Equal(TransferStatus.Completed, (await db.Transfers.FindAsync(1))!.Status);
    }

    // These two handlers Include() every navigation on Transfer (Asset, TargetUser,
    // CurrentHolder, TransferBy, ToDivision/FromDivision) before projecting to
    // TransferDto, so — unlike the Count-only handler below — they need every
    // referenced row actually seeded, not just the Transfer's own FK columns.
    private static async Task SeedNavigationsAsync(Assura.Application.Tests.Common.TestApplicationDbContext db, int targetUserId, int currentHolderId, int fromDivisionId, int toDivisionId)
    {
        db.Assets.Add(new Asset { Id = 1, AssetCode = "A1", Status = AssetStatus.Transferred, AssignedUserId = targetUserId });
        db.AssetRequests.Add(new AssetRequest { Id = 1, AssetName = "Laptop", Priority = "Normal", RequestType = "Transfer", RequesterId = targetUserId.ToString() });
        db.Divisions.Add(new Division { Id = fromDivisionId, Name = $"Division {fromDivisionId}" });
        db.Divisions.Add(new Division { Id = toDivisionId, Name = $"Division {toDivisionId}" });
        db.Users.Add(new User { Id = targetUserId, Role = UserRole.Employee, DivisionId = toDivisionId });
        db.Users.Add(new User { Id = currentHolderId, Role = UserRole.Employee, DivisionId = fromDivisionId });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetEmployeeTransferQuery_ActiveTab_IncludesOverdue()
    {
        using var db = TestContextFactory.CreateContext();
        await SeedNavigationsAsync(db, targetUserId: 50, currentHolderId: 55, fromDivisionId: 5, toDivisionId: 9);
        db.Transfers.Add(MakeOverdueTransfer(2, targetUserId: 50, currentHolderId: 55, fromDivisionId: 5, toDivisionId: 9));
        await db.SaveChangesAsync();

        var handler = new GetEmployeeTransferQueryHandler(db);
        var result = await handler.Handle(new GetEmployeeTransferQuery("active", 50), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Overdue", result[0].Status);
    }

    [Fact]
    public async Task GetDivisionHeadTransferQuery_ActiveTab_IncludesOverdue()
    {
        using var db = TestContextFactory.CreateContext();
        await SeedNavigationsAsync(db, targetUserId: 50, currentHolderId: 55, fromDivisionId: 5, toDivisionId: 9);
        db.Transfers.Add(MakeOverdueTransfer(3, targetUserId: 50, currentHolderId: 55, fromDivisionId: 5, toDivisionId: 9));
        db.Users.Add(new User { Id = 200, Role = UserRole.DivisionHead, DivisionId = 9 });
        await db.SaveChangesAsync();

        var handler = new GetDivisionHeadTransferQueryHandler(db);
        var result = await handler.Handle(new GetDivisionHeadTransferQuery("active", 200), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Overdue", result[0].Status);
    }

    [Fact]
    public async Task GetTransferCounts_DivisionHead_ActiveCount_IncludesOverdue()
    {
        using var db = TestContextFactory.CreateContext();
        db.Transfers.Add(MakeOverdueTransfer(4, targetUserId: 50, currentHolderId: 55, fromDivisionId: 5, toDivisionId: 9));
        db.Users.Add(new User { Id = 201, Role = UserRole.DivisionHead, DivisionId = 9 });
        await db.SaveChangesAsync();

        var handler = new GetTransferCountsQueryHandler(db);
        var counts = await handler.Handle(new GetTransferCountsQuery(201), CancellationToken.None);

        Assert.Equal(1, counts.ActiveCount);
    }

    [Fact]
    public async Task GetTransferCounts_Employee_ActiveCount_IncludesOverdue()
    {
        using var db = TestContextFactory.CreateContext();
        db.Transfers.Add(MakeOverdueTransfer(5, targetUserId: 50, currentHolderId: 55, fromDivisionId: 5, toDivisionId: 9));
        db.Users.Add(new User { Id = 50, Role = UserRole.Employee, DivisionId = 9 });
        await db.SaveChangesAsync();

        var handler = new GetTransferCountsQueryHandler(db);
        var counts = await handler.Handle(new GetTransferCountsQuery(50), CancellationToken.None);

        Assert.Equal(1, counts.ActiveCount);
    }
}
