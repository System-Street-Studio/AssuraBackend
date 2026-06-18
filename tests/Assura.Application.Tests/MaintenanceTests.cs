using Assura.Application.Features.Maintenances.Commands;
using Assura.Application.Features.Maintenances.Queries;
using Assura.Application.Tests.Common;
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

    private static TestApplicationDbContext CreateContext() => TestContextFactory.CreateContext();
}
