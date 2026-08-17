using Assura.Application.Features.Maintenances.Commands;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Assura.Application.Tests;

// Covers a bug found by the test-workflow simulation: MaintenancesController's
// class-level [Authorize] only checked role, not division, so any Division Head
// could approve/reject/start/complete/escalate a maintenance record belonging to a
// completely different division (reproduced live: an Industrial Services Division
// Head was able to reject a maintenance record for an Information Technology
// division asset). AssetRequestsController's ApproveAssetRequestCommand already had
// the correct pattern (compare caller's DivisionId to the entity's division;
// Admin/other privileged roles bypass); these tests mirror that pattern applied to
// Maintenance's five mutating command handlers, scoped against the maintenance
// record's Asset.DivisionId since Maintenance has no DivisionId of its own and
// RequestedByUserId is null for ad-hoc (Procurement-created) records.
public class MaintenanceDivisionScopingTests
{
    private static (Division itDivision, Division industrialDivision, User itHead, User industrialHead, Asset itAsset) SeedTwoDivisions(TestApplicationDbContext db)
    {
        var itDivision = new Division { Id = 1, Name = "Information Technology" };
        var industrialDivision = new Division { Id = 2, Name = "Industrial Services" };
        var itHead = new User { Id = 100, FirstName = "IT", LastName = "Head", Role = UserRole.DivisionHead, DivisionId = itDivision.Id };
        var industrialHead = new User { Id = 101, FirstName = "Industrial", LastName = "Head", Role = UserRole.DivisionHead, DivisionId = industrialDivision.Id };
        var itAsset = new Asset { Id = 200, AssetCode = "AST-200", DivisionId = itDivision.Id };

        db.Divisions.AddRange(itDivision, industrialDivision);
        db.Users.AddRange(itHead, industrialHead);
        db.Assets.Add(itAsset);

        return (itDivision, industrialDivision, itHead, industrialHead, itAsset);
    }

    [Fact]
    public async Task UpdateMaintenanceStatus_CrossDivisionHead_ThrowsUnauthorized()
    {
        using var db = TestContextFactory.CreateContext();
        var (_, _, _, industrialHead, itAsset) = SeedTwoDivisions(db);
        db.Maintenances.Add(new Maintenance { Id = 300, MaintenanceNumber = "MNT-300", Type = MaintenanceType.Corrective, MaintenanceDate = DateTime.UtcNow, Status = "PendingApproval", AssetId = itAsset.Id });
        await db.SaveChangesAsync();

        var handler = new UpdateMaintenanceStatusCommandHandler(db, NullLogger<UpdateMaintenanceStatusCommandHandler>.Instance);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new UpdateMaintenanceStatusCommand(300, "Approved", industrialHead.Id, IsDivisionHead: true), CancellationToken.None));
    }

    [Fact]
    public async Task UpdateMaintenanceStatus_SameDivisionHead_Succeeds()
    {
        using var db = TestContextFactory.CreateContext();
        var (_, _, itHead, _, itAsset) = SeedTwoDivisions(db);
        db.Maintenances.Add(new Maintenance { Id = 301, MaintenanceNumber = "MNT-301", Type = MaintenanceType.Corrective, MaintenanceDate = DateTime.UtcNow, Status = "PendingApproval", AssetId = itAsset.Id });
        await db.SaveChangesAsync();

        var handler = new UpdateMaintenanceStatusCommandHandler(db, NullLogger<UpdateMaintenanceStatusCommandHandler>.Instance);
        await handler.Handle(new UpdateMaintenanceStatusCommand(301, "Approved", itHead.Id, IsDivisionHead: true), CancellationToken.None);

        var maintenance = await db.Maintenances.FirstAsync(m => m.Id == 301);
        Assert.Equal("Approved", maintenance.Status);
        Assert.Equal(itHead.Id, maintenance.ApprovedByUserId);
    }

    [Fact]
    public async Task UpdateMaintenanceStatus_NonDivisionHeadCaller_BypassesDivisionCheck()
    {
        using var db = TestContextFactory.CreateContext();
        var (_, _, _, _, itAsset) = SeedTwoDivisions(db);
        var storekeeper = new User { Id = 102, FirstName = "Store", LastName = "Keeper", Role = UserRole.Storekeeper };
        db.Users.Add(storekeeper);
        db.Maintenances.Add(new Maintenance { Id = 302, MaintenanceNumber = "MNT-302", Type = MaintenanceType.Corrective, MaintenanceDate = DateTime.UtcNow, Status = "Approved", AssetId = itAsset.Id });
        await db.SaveChangesAsync();

        var handler = new UpdateMaintenanceStatusCommandHandler(db, NullLogger<UpdateMaintenanceStatusCommandHandler>.Instance);
        // Storekeeper has no DivisionId at all here, but IsDivisionHead is false, so the
        // scoping check must never run for them (matches Admin's bypass in ApproveAssetRequestCommand).
        await handler.Handle(new UpdateMaintenanceStatusCommand(302, "InProgress", storekeeper.Id, IsDivisionHead: false), CancellationToken.None);

        var maintenance = await db.Maintenances.FirstAsync(m => m.Id == 302);
        Assert.Equal("InProgress", maintenance.Status);
    }

    [Fact]
    public async Task AssignTemporaryAsset_CrossDivisionHead_ThrowsUnauthorized()
    {
        using var db = TestContextFactory.CreateContext();
        var (_, _, _, industrialHead, itAsset) = SeedTwoDivisions(db);
        var replacement = new Asset { Id = 201, AssetCode = "AST-201", Status = AssetStatus.InStore };
        db.Assets.Add(replacement);
        db.Maintenances.Add(new Maintenance { Id = 303, MaintenanceNumber = "MNT-303", Type = MaintenanceType.Corrective, MaintenanceDate = DateTime.UtcNow, Status = "InProgress", AssetId = itAsset.Id });
        await db.SaveChangesAsync();

        var handler = new AssignTemporaryAssetCommandHandler(db, NullLogger<AssignTemporaryAssetCommandHandler>.Instance);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new AssignTemporaryAssetCommand { MaintenanceId = 303, ReplacementAssetId = replacement.Id, StorekeeperUserId = industrialHead.Id, IsDivisionHead = true }, CancellationToken.None));
    }

    [Fact]
    public async Task SendForRepair_CrossDivisionHead_ThrowsUnauthorized()
    {
        using var db = TestContextFactory.CreateContext();
        var (_, _, _, industrialHead, itAsset) = SeedTwoDivisions(db);
        db.Maintenances.Add(new Maintenance { Id = 304, MaintenanceNumber = "MNT-304", Type = MaintenanceType.Corrective, MaintenanceDate = DateTime.UtcNow, Status = "InProgress", AssetId = itAsset.Id });
        await db.SaveChangesAsync();

        var handler = new SendForRepairCommandHandler(db, NullLogger<SendForRepairCommandHandler>.Instance);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new SendForRepairCommand { MaintenanceId = 304, StorekeeperUserId = industrialHead.Id, IsDivisionHead = true }, CancellationToken.None));
    }

    [Fact]
    public async Task EscalateToProcurement_CrossDivisionHead_ThrowsUnauthorized()
    {
        using var db = TestContextFactory.CreateContext();
        var (_, _, _, industrialHead, itAsset) = SeedTwoDivisions(db);
        db.Maintenances.Add(new Maintenance { Id = 305, MaintenanceNumber = "MNT-305", Type = MaintenanceType.Corrective, MaintenanceDate = DateTime.UtcNow, Status = "InProgress", AssetId = itAsset.Id });
        await db.SaveChangesAsync();

        var handler = new EscalateToProcurementCommandHandler(db, NullLogger<EscalateToProcurementCommandHandler>.Instance);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new EscalateToProcurementCommand { MaintenanceId = 305, StorekeeperUserId = industrialHead.Id, IsDivisionHead = true }, CancellationToken.None));
    }

    // Direct reproduction of the live finding: a Division Head from an unrelated
    // division rejecting a maintenance record for another division's asset.
    [Fact]
    public async Task RejectMaintenance_CrossDivisionHead_ThrowsUnauthorized()
    {
        using var db = TestContextFactory.CreateContext();
        var (_, _, _, industrialHead, itAsset) = SeedTwoDivisions(db);
        db.Maintenances.Add(new Maintenance { Id = 306, MaintenanceNumber = "MNT-306", Type = MaintenanceType.Corrective, MaintenanceDate = DateTime.UtcNow, Status = "Approved", AssetId = itAsset.Id });
        await db.SaveChangesAsync();

        var handler = new RejectMaintenanceCommandHandler(db, NullLogger<RejectMaintenanceCommandHandler>.Instance);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new RejectMaintenanceCommand { MaintenanceId = 306, RejectedByUserId = industrialHead.Id, IsDivisionHead = true, Reason = "cross-division test" }, CancellationToken.None));

        // The record must be left untouched by the rejected attempt.
        var maintenance = await db.Maintenances.FirstAsync(m => m.Id == 306);
        Assert.Equal("Approved", maintenance.Status);
    }

    [Fact]
    public async Task RejectMaintenance_SameDivisionHead_Succeeds()
    {
        using var db = TestContextFactory.CreateContext();
        var (_, _, itHead, _, itAsset) = SeedTwoDivisions(db);
        db.Maintenances.Add(new Maintenance { Id = 307, MaintenanceNumber = "MNT-307", Type = MaintenanceType.Corrective, MaintenanceDate = DateTime.UtcNow, Status = "Approved", AssetId = itAsset.Id });
        await db.SaveChangesAsync();

        var handler = new RejectMaintenanceCommandHandler(db, NullLogger<RejectMaintenanceCommandHandler>.Instance);
        await handler.Handle(new RejectMaintenanceCommand { MaintenanceId = 307, RejectedByUserId = itHead.Id, IsDivisionHead = true, Reason = "same-division test" }, CancellationToken.None);

        var maintenance = await db.Maintenances.FirstAsync(m => m.Id == 307);
        Assert.Equal("Rejected", maintenance.Status);
    }
}
