using Assura.Application.PurchasingOrders.Commands;
using Assura.Application.PurchasingOrders.Queries;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Tests;

public class ProcurementTests
{
    [Fact]
    public async Task CreatePurchasingOrder_NewSupplier_ShouldCreateSupplierAndPO()
    {
        using var db = CreateContext();
        var handler = new CreatePurchasingOrderCommandHandler(db);

        var command = new CreatePurchasingOrderCommand
        {
            SupplierName = "New Global Tech",
            Items = new List<CreatePurchasingOrderItemDto>
            {
                new CreatePurchasingOrderItemDto
                {
                    ItemName = "High-End Laptop",
                    Quantity = 2,
                    UnitPrice = 150000,
                    VatPercentage = 15,
                    Discount = 10
                }
            }
        };

        var poId = await handler.Handle(command, CancellationToken.None);

        var po = await db.PurchasingOrders
            .Include(p => p.Supplier)
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == poId);

        Assert.NotNull(po);
        Assert.Equal("New Global Tech", po!.Supplier.Name);
        Assert.Single(po.Items);
        
        // Calculation check:
        // Amount = 2 * 150000 = 300000
        // Discount 10% = 30000
        // DiscountedPrice = 270000
        // VAT 15% = 270000 * 0.15 = 40500
        // TotalPrice = 270000 + 40500 = 310500
        Assert.Equal(310500, po.TotalAmount);
    }

    [Fact]
    public async Task GetProcurementStats_ShouldReturnCorrectCounts()
    {
        using var db = CreateContext();
        
        db.Suppliers.Add(new Supplier { Name = "S1" });
        db.Suppliers.Add(new Supplier { Name = "S2" });
        
        db.PurchasingOrders.Add(new PurchasingOrder { Status = "Completed", SupplierId = 1, OrderNumber = "PO1" });
        db.PurchasingOrders.Add(new PurchasingOrder { Status = "Pending", SupplierId = 2, OrderNumber = "PO2" });

        db.Maintenances.Add(new Maintenance { Status = "Completed", AssetId = 1 });
        db.Maintenances.Add(new Maintenance { Status = "In Progress", AssetId = 2 });

        await db.SaveChangesAsync();

        var handler = new GetProcurementStatsQueryHandler(db);
        var stats = await handler.Handle(new GetProcurementStatsQuery(), CancellationToken.None);

        Assert.Equal(2, stats.TotalSuppliers);
        Assert.Equal(1, stats.PosCompleted);
        Assert.Equal(1, stats.PosNotCompleted);
        Assert.Equal(1, stats.RepairsCompleted);
        Assert.Equal(1, stats.RepairsNotCompleted);
    }

    [Fact]
    public async Task GetPendingAssetRequests_ShouldReturnOnlyPendingRequests()
    {
        using var db = CreateContext();
        var requester = new User { Id = 1, FirstName = "John", LastName = "Doe" };
        db.Users.Add(requester);

        db.Requests.Add(new Request { Status = "Pending", Requester = requester, CreatedAt = DateTime.UtcNow, Specifications = "High-end PC" });
        db.Requests.Add(new Request { Status = "Approved", Requester = requester, CreatedAt = DateTime.UtcNow, Specifications = "Mouse" });
        await db.SaveChangesAsync();

        var handler = new GetPendingAssetRequestsQueryHandler(db);
        var result = await handler.Handle(new GetPendingAssetRequestsQuery(), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("High-end PC", result[0].Specifications);
    }

    private static TestApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestApplicationDbContext(options);
    }
}
