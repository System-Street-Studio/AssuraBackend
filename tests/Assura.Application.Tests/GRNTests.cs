using Assura.Application.Features.GRNs.Commands;
using Assura.Application.Features.GRNs.Queries;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using FluentValidation;

namespace Assura.Application.Tests;

// Covers the BUGS.md Storekeeper finding: "GRN/GIN/TIN inventory documentation
// is entirely missing." This exercises the new GRN (Goods Received Note)
// vertical slice: record one against a purchasing order + asset, then list/view it.
public class GRNTests
{
    [Fact]
    public async Task CreateGRN_ShouldPersistAndReturnDto()
    {
        using var db = TestContextFactory.CreateContext();

        var supplier = new Supplier { Name = "Apex Procurement Co." };
        var product = new Product { Name = "Office Chair Ergonomic" };
        var category = new Category { Name = "Furniture & Fittings" };
        db.Suppliers.Add(supplier);
        db.Products.Add(product);
        db.Categories.Add(category);

        var po = new PurchasingOrder { OrderNumber = "PO-0001", OrderDate = DateTime.UtcNow, Supplier = supplier };
        db.PurchasingOrders.Add(po);

        var asset = new Asset { AssetCode = "AST-0099", Product = product, Category = category };
        db.Assets.Add(asset);
        await db.SaveChangesAsync();

        var handler = new CreateGRNCommandHandler(db);
        var command = new CreateGRNCommand(po.Id, asset.Id, DateTime.UtcNow, "Stores Employee", "Received in good condition");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.StartsWith("GRN-", result.GrnNumber);
        Assert.Equal("PO-0001", result.PurchasingOrderNumber);
        Assert.Equal("Apex Procurement Co.", result.SupplierName);
        Assert.Equal("AST-0099", result.AssetCode);
        Assert.Single(db.GRNs);
    }

    [Fact]
    public async Task CreateGRN_ShouldRejectDuplicateForSameAsset()
    {
        using var db = TestContextFactory.CreateContext();

        var supplier = new Supplier { Name = "Apex Procurement Co." };
        var product = new Product { Name = "Office Chair Ergonomic" };
        db.Suppliers.Add(supplier);
        db.Products.Add(product);

        var po = new PurchasingOrder { OrderNumber = "PO-0001", OrderDate = DateTime.UtcNow, Supplier = supplier };
        db.PurchasingOrders.Add(po);

        var asset = new Asset { AssetCode = "AST-0099", Product = product };
        db.Assets.Add(asset);
        db.GRNs.Add(new GRN { GrnNumber = "GRN-EXISTING", ReceivedDate = DateTime.UtcNow, PurchasingOrder = po, Asset = asset });
        await db.SaveChangesAsync();

        var handler = new CreateGRNCommandHandler(db);
        var command = new CreateGRNCommand(po.Id, asset.Id, DateTime.UtcNow, null, null);

        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task GetGRNs_ShouldReturnRecordedGRNs()
    {
        using var db = TestContextFactory.CreateContext();

        var supplier = new Supplier { Name = "Apex Procurement Co." };
        var product = new Product { Name = "Office Chair Ergonomic" };
        db.Suppliers.Add(supplier);
        db.Products.Add(product);

        var po = new PurchasingOrder { OrderNumber = "PO-0001", OrderDate = DateTime.UtcNow, Supplier = supplier };
        db.PurchasingOrders.Add(po);

        var asset = new Asset { AssetCode = "AST-0099", Product = product };
        db.Assets.Add(asset);
        db.GRNs.Add(new GRN { GrnNumber = "GRN-0001", ReceivedDate = DateTime.UtcNow, ReceivedBy = "Stores Employee", PurchasingOrder = po, Asset = asset });
        await db.SaveChangesAsync();

        var handler = new GetGRNsQueryHandler(db);
        var result = await handler.Handle(new GetGRNsQuery(), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("GRN-0001", result[0].GrnNumber);
        Assert.Equal("Apex Procurement Co.", result[0].SupplierName);
    }
}
