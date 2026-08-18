using Assura.Application.Features.Maintenances.Commands;
using Assura.Application.Features.Maintenances.Queries;
using Assura.Application.Tests.Common;
using Assura.Domain.Constants;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Assura.Application.Tests;

public class MaintenanceTests
{
    [Fact]
    public async Task GetMaintenances_ShouldReturnAllRecords()
    {
        using var db = CreateContext();
        
        var product = new Product { Name = "Laptop X" };
        var category = new Category { Name = "Laptops" };
        var division = new Division { Name = "IT" };
        var supplier = new Supplier { Name = "Supplier A" };
        db.Products.Add(product);
        db.Categories.Add(category);
        db.Divisions.Add(division);
        db.Suppliers.Add(supplier);

        var asset = new Asset 
        { 
            AssetCode = "AST001",
            Product = product,
            Category = category,
            Division = division,
            Supplier = supplier,
            Status = AssetStatus.UnderMaintenance
        };
        db.Assets.Add(asset);

        db.Maintenances.Add(new Maintenance 
        { 
            MaintenanceNumber = "MNT001",
            Asset = asset,
            Type = MaintenanceType.Corrective,
            Status = "In Progress",
            MaintenanceDate = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var handler = new GetMaintenancesQueryHandler(db, NullLogger<GetMaintenancesQueryHandler>.Instance);
        var result = await handler.Handle(new GetMaintenancesQuery(), CancellationToken.None);

        Assert.NotEmpty(result);
        Assert.Equal("MNT001", result[0].MaintenanceNumber);
        Assert.Equal("Laptop X", result[0].AssetName);
    }

    [Fact]
    public async Task CreateMaintenance_ShouldAddRecordToDb()
    {
        using var db = CreateContext();
        var handler = new CreateMaintenanceCommandHandler(db, NullLogger<CreateMaintenanceCommandHandler>.Instance);

        var command = new CreateMaintenanceCommand
        {
            AssetId = 1,
            Type = MaintenanceType.Preventive,
            Description = "General service",
            MaintenanceDate = DateTime.UtcNow
        };

        var id = await handler.Handle(command, CancellationToken.None);

        var maintenance = await db.Maintenances.FindAsync(id);
        Assert.NotNull(maintenance);
        Assert.Equal("General service", maintenance!.Description);
    }

    // Covers a user-reported bug: after Procurement creates a Maintenance Note for a
    // queue item, that item stayed stuck in the "PendingProcurement" pending-requests
    // queue forever, because CreateMaintenanceCommand never told the originating
    // Request/AssetRequest it had been resolved.
    [Fact]
    public async Task CreateMaintenance_WithRequestId_ClearsMatchingRequestFromProcurementQueue()
    {
        using var db = CreateContext();
        var handler = new CreateMaintenanceCommandHandler(db, NullLogger<CreateMaintenanceCommandHandler>.Instance);

        var requester = new User { Id = 1, FirstName = "IT", LastName = "Employee" };
        db.Users.Add(requester);
        var pendingRequest = new Request
        {
            Id = 100,
            RequesterId = requester.Id,
            RequestNumber = "REQ-100",
            Status = RequestWorkflowStatus.PendingProcurement
        };
        db.Requests.Add(pendingRequest);
        await db.SaveChangesAsync();

        var command = new CreateMaintenanceCommand
        {
            AssetId = 1,
            Type = MaintenanceType.Preventive,
            Description = "Screen replacement",
            MaintenanceDate = DateTime.UtcNow,
            RequestId = pendingRequest.Id
        };

        await handler.Handle(command, CancellationToken.None);

        var updated = await db.Requests.FirstAsync(r => r.Id == pendingRequest.Id);
        Assert.NotEqual(RequestWorkflowStatus.PendingProcurement, updated.Status);
    }

    [Fact]
    public async Task CreateMaintenance_WithRequestId_ClearsMatchingAssetRequestFromProcurementQueue()
    {
        using var db = CreateContext();
        var handler = new CreateMaintenanceCommandHandler(db, NullLogger<CreateMaintenanceCommandHandler>.Instance);

        var pendingAssetRequest = new AssetRequest
        {
            Id = 200,
            RequesterId = "1",
            RequesterName = "IT Employee",
            AssetName = "Broken Printer",
            Priority = "Normal",
            RequestType = "Maintenance",
            Status = RequestStatus.PendingProcurement
        };
        db.AssetRequests.Add(pendingAssetRequest);
        await db.SaveChangesAsync();

        var command = new CreateMaintenanceCommand
        {
            AssetId = 1,
            Type = MaintenanceType.Preventive,
            Description = "Printer repair",
            MaintenanceDate = DateTime.UtcNow,
            RequestId = pendingAssetRequest.Id
        };

        await handler.Handle(command, CancellationToken.None);

        var updated = await db.AssetRequests.FirstAsync(r => r.Id == pendingAssetRequest.Id);
        Assert.Equal(RequestStatus.Passed, updated.Status);
    }

    // Covers the requested Storekeeper workflow: once Procurement marks a Maintenance
    // Note "Completed", the Storekeeper should be able to inform both the requesting
    // employee and their Division Head, which also flips the record to "Submitted".
    [Fact]
    public async Task InformStakeholders_OnCompletedMaintenance_NotifiesEmployeeAndDivisionHeadAndSubmits()
    {
        using var db = TestContextFactory.CreateContext();

        var division = new Division { Id = 1, Name = "IT" };
        var employee = new User { Id = 10, FirstName = "IT", LastName = "Employee", Role = UserRole.Employee, DivisionId = division.Id };
        var head = new User { Id = 11, FirstName = "Division", LastName = "Head", Role = UserRole.DivisionHead, DivisionId = division.Id };
        var storekeeper = new User { Id = 12, FirstName = "Store", LastName = "Keeper", Role = UserRole.Storekeeper };
        var asset = new Asset { Id = 20, AssetCode = "AST-20", DivisionId = division.Id, Status = AssetStatus.UnderMaintenance, AssignedUserId = null };
        var tempAsset = new Asset { Id = 21, AssetCode = "AST-TEMP", DivisionId = division.Id, Status = AssetStatus.InUse, AssignedUserId = employee.Id };
        var assetRequest = new AssetRequest { Id = 50, RequesterId = "10", AssetName = "AST-20", RequestType = "Maintenance", Status = RequestStatus.Pending, AssetId = asset.Id };

        db.Divisions.Add(division);
        db.Users.AddRange(employee, head, storekeeper);
        db.Assets.AddRange(asset, tempAsset);
        db.AssetRequests.Add(assetRequest);
        db.Maintenances.Add(new Maintenance
        {
            Id = 30,
            MaintenanceNumber = "MNT-30",
            Type = MaintenanceType.Corrective,
            MaintenanceDate = DateTime.UtcNow,
            Status = "Completed",
            AssetId = asset.Id,
            RequestedByUserId = employee.Id,
            ReplacementAssetId = tempAsset.Id,
            OriginalRequestId = assetRequest.Id
        });
        await db.SaveChangesAsync();

        var handler = new InformMaintenanceStakeholdersCommandHandler(db, Microsoft.Extensions.Logging.Abstractions.NullLogger<InformMaintenanceStakeholdersCommandHandler>.Instance);
        var result = await handler.Handle(new InformMaintenanceStakeholdersCommand { MaintenanceId = 30, StorekeeperUserId = storekeeper.Id }, CancellationToken.None);

        Assert.Equal(InformMaintenanceStakeholdersResult.Success, result);

        var maintenance = await db.Maintenances.FirstAsync(m => m.Id == 30);
        Assert.Equal("Submitted", maintenance.Status);
        Assert.Equal(storekeeper.Id, maintenance.StorekeeperUserId);

        // Verify primary asset is reactivated and assigned back to employee
        var updatedAsset = await db.Assets.FirstAsync(a => a.Id == 20);
        Assert.Equal(AssetStatus.InUse, updatedAsset.Status);
        Assert.Equal(employee.Id, updatedAsset.AssignedUserId);

        // Verify temporary asset is returned to store
        var updatedTempAsset = await db.Assets.FirstAsync(a => a.Id == 21);
        Assert.Equal(AssetStatus.InStore, updatedTempAsset.Status);
        Assert.Null(updatedTempAsset.AssignedUserId);

        // Verify asset request is marked completed
        var updatedAssetRequest = await db.AssetRequests.FirstAsync(ar => ar.Id == 50);
        Assert.Equal(RequestStatus.Completed, updatedAssetRequest.Status);

        Assert.NotNull(await db.Notifications.FirstOrDefaultAsync(n => n.UserId == employee.Id));
        Assert.NotNull(await db.Notifications.FirstOrDefaultAsync(n => n.UserId == head.Id));
    }

    [Fact]
    public async Task InformStakeholders_OnNonCompletedMaintenance_ReturnsInvalidStatus()
    {
        using var db = TestContextFactory.CreateContext();

        var asset = new Asset { Id = 21, AssetCode = "AST-21" };
        db.Assets.Add(asset);
        db.Maintenances.Add(new Maintenance
        {
            Id = 31,
            MaintenanceNumber = "MNT-31",
            Type = MaintenanceType.Corrective,
            MaintenanceDate = DateTime.UtcNow,
            Status = "InProgress",
            AssetId = asset.Id
        });
        await db.SaveChangesAsync();

        var handler = new InformMaintenanceStakeholdersCommandHandler(db, Microsoft.Extensions.Logging.Abstractions.NullLogger<InformMaintenanceStakeholdersCommandHandler>.Instance);
        var result = await handler.Handle(new InformMaintenanceStakeholdersCommand { MaintenanceId = 31, StorekeeperUserId = 1 }, CancellationToken.None);

        Assert.Equal(InformMaintenanceStakeholdersResult.InvalidStatus, result);
        Assert.Empty(db.Notifications);
    }

    [Fact]
    public async Task GetMaintenanceStats_ShouldCountLegacyStatusFormatting()
    {
        using var db = CreateContext();

        var product = new Product { Name = "Office Chair Ergonomic" };
        var category = new Category { Name = "Furniture & Fittings" };
        db.Products.Add(product);
        db.Categories.Add(category);

        var asset = new Asset { AssetCode = "AST001", Product = product, Category = category, Status = AssetStatus.UnderMaintenance };
        db.Assets.Add(asset);

        // Reproduces the live-data mix found in the Storekeeper simulation:
        // "In Progress" (spaced) and "Pending" (legacy short form) alongside
        // the canonical "InProgress"/"PendingApproval" the frontend writes.
        db.Maintenances.AddRange(
            new Maintenance { MaintenanceNumber = "MNT-A", Asset = asset, Type = MaintenanceType.Corrective, Status = "In Progress", MaintenanceDate = DateTime.UtcNow },
            new Maintenance { MaintenanceNumber = "MNT-B", Asset = asset, Type = MaintenanceType.Corrective, Status = "InProgress", MaintenanceDate = DateTime.UtcNow },
            new Maintenance { MaintenanceNumber = "MNT-C", Asset = asset, Type = MaintenanceType.Corrective, Status = "Pending", MaintenanceDate = DateTime.UtcNow },
            new Maintenance { MaintenanceNumber = "MNT-D", Asset = asset, Type = MaintenanceType.Corrective, Status = "PendingApproval", MaintenanceDate = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var handler = new GetMaintenanceStatsQueryHandler(db);
        var stats = await handler.Handle(new GetMaintenanceStatsQuery(), CancellationToken.None);

        Assert.Equal(4, stats.Total);
        Assert.Equal(2, stats.InProgress);
        Assert.Equal(2, stats.PendingApproval);
    }

    // Covers a bug found by the test-workflow simulation: Procurement's maintenance
    // note-creation form/endpoint accepted submission without a Repair Firm, even
    // though the form marks it as required in the UI convention used by every other
    // field on the page. CreateMaintenanceCommand had no FluentValidation validator
    // at all.
    [Fact]
    public void CreateMaintenanceCommandValidator_WithoutRepairingFirmId_Fails()
    {
        var validator = new CreateMaintenanceCommandValidator();
        var command = new CreateMaintenanceCommand
        {
            MaintenanceNumber = "MTN-1",
            Type = MaintenanceType.Preventive,
            MaintenanceDate = DateTime.UtcNow,
            Description = "Screen replacement",
            Cost = 100,
            Status = "Scheduled",
            AssetId = 1,
            RepairingFirmId = null
        };

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateMaintenanceCommand.RepairingFirmId));
    }

    [Fact]
    public void CreateMaintenanceCommandValidator_WithRepairingFirmId_Passes()
    {
        var validator = new CreateMaintenanceCommandValidator();
        var command = new CreateMaintenanceCommand
        {
            MaintenanceNumber = "MTN-1",
            Type = MaintenanceType.Preventive,
            MaintenanceDate = DateTime.UtcNow,
            Description = "Screen replacement",
            Cost = 100,
            Status = "Scheduled",
            AssetId = 1,
            RepairingFirmId = 5
        };

        Assert.True(validator.Validate(command).IsValid);
    }

    // Confirms every field submitted by the maintenance note-creation form actually
    // persists on the created Maintenance record (empirically verified live against
    // the running app before writing this; nothing was actually lost at the
    // handler/entity layer — the reported "fields not appearing" symptom traced back
    // to a frontend status-string mismatch, not a backend mapping gap).
    [Fact]
    public async Task CreateMaintenance_AllFormFields_PersistCorrectly()
    {
        using var db = CreateContext();
        var handler = new CreateMaintenanceCommandHandler(db, NullLogger<CreateMaintenanceCommandHandler>.Instance);

        var asset = new Asset { Id = 500, AssetCode = "AST-500" };
        var firm = new RepairingFirm { Id = 7, Name = "Acme Repairs" };
        db.Assets.Add(asset);
        db.RepairingFirms.Add(firm);
        await db.SaveChangesAsync();

        var command = new CreateMaintenanceCommand
        {
            MaintenanceNumber = "MTN-ALLFIELDS",
            Type = MaintenanceType.Preventive,
            MaintenanceDate = new DateTime(2026, 8, 16),
            Description = "Full field test description",
            Cost = 1234.56m,
            Status = "InProgress",
            AssetId = asset.Id,
            RepairingFirmId = firm.Id
        };

        var id = await handler.Handle(command, CancellationToken.None);

        var maintenance = await db.Maintenances.FindAsync(id);
        Assert.NotNull(maintenance);
        Assert.Equal("MTN-ALLFIELDS", maintenance!.MaintenanceNumber);
        Assert.Equal(MaintenanceType.Preventive, maintenance.Type);
        Assert.Equal(new DateTime(2026, 8, 16), maintenance.MaintenanceDate);
        Assert.Equal("Full field test description", maintenance.Description);
        Assert.Equal(1234.56m, maintenance.Cost);
        Assert.Equal("InProgress", maintenance.Status);
        Assert.Equal(asset.Id, maintenance.AssetId);
        Assert.Equal(firm.Id, maintenance.RepairingFirmId);
    }

    private static TestApplicationDbContext CreateContext() => TestContextFactory.CreateContext();
}
