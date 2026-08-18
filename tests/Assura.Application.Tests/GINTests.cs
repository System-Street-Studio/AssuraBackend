using Assura.Application.Features.GINs.Commands;
using Assura.Application.Features.GINs.Queries;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using FluentValidation;

namespace Assura.Application.Tests;

// Covers the test-workflow finding: GIN (Goods Issue Note) had a domain entity and
// DB migrations from the same original effort that built GRN ("GRN/GIN/TIN
// inventory documentation is entirely missing" in BUGS.md), but only GRN was ever
// finished — GIN had no controller, commands, or queries at all. This exercises the
// new GIN vertical slice: record one against the GRN the asset arrived under, then
// list/view it.
public class GINTests
{
    [Fact]
    public async Task CreateGIN_ShouldPersistAndReturnDto()
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

        var grn = new GRN { GrnNumber = "GRN-0001", ReceivedDate = DateTime.UtcNow, PurchasingOrder = po, Asset = asset };
        db.GRNs.Add(grn);
        await db.SaveChangesAsync();

        var handler = new CreateGINCommandHandler(db);
        var command = new CreateGINCommand(grn.Id, asset.Id, DateTime.UtcNow, "Good", "Issued to IT division");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.StartsWith("GIN-", result.GinNumber);
        Assert.Equal("GRN-0001", result.GrnNumber);
        Assert.Equal("AST-0099", result.AssetCode);
        Assert.Single(db.GINs);
    }

    [Fact]
    public async Task CreateGIN_ShouldRejectMismatchedAssetAndGrn()
    {
        using var db = TestContextFactory.CreateContext();

        var supplier = new Supplier { Name = "Apex Procurement Co." };
        db.Suppliers.Add(supplier);
        var po = new PurchasingOrder { OrderNumber = "PO-0001", OrderDate = DateTime.UtcNow, Supplier = supplier };
        db.PurchasingOrders.Add(po);

        var receivedAsset = new Asset { AssetCode = "AST-0099" };
        var otherAsset = new Asset { AssetCode = "AST-0100" };
        db.Assets.AddRange(receivedAsset, otherAsset);

        var grn = new GRN { GrnNumber = "GRN-0001", ReceivedDate = DateTime.UtcNow, PurchasingOrder = po, Asset = receivedAsset };
        db.GRNs.Add(grn);
        await db.SaveChangesAsync();

        var handler = new CreateGINCommandHandler(db);
        // otherAsset was never received under this GRN — issuing it against this
        // GRN's paperwork would misrepresent which physical item left the store.
        var command = new CreateGINCommand(grn.Id, otherAsset.Id, DateTime.UtcNow, null, null);

        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task CreateGIN_ShouldRejectDuplicateForSameAsset()
    {
        using var db = TestContextFactory.CreateContext();

        var supplier = new Supplier { Name = "Apex Procurement Co." };
        db.Suppliers.Add(supplier);
        var po = new PurchasingOrder { OrderNumber = "PO-0001", OrderDate = DateTime.UtcNow, Supplier = supplier };
        db.PurchasingOrders.Add(po);

        var asset = new Asset { AssetCode = "AST-0099" };
        db.Assets.Add(asset);

        var grn = new GRN { GrnNumber = "GRN-0001", ReceivedDate = DateTime.UtcNow, PurchasingOrder = po, Asset = asset };
        db.GRNs.Add(grn);
        db.GINs.Add(new GIN { GinNumber = "GIN-EXISTING", AssignedDate = DateTime.UtcNow, GRN = grn, Asset = asset });
        await db.SaveChangesAsync();

        var handler = new CreateGINCommandHandler(db);
        var command = new CreateGINCommand(grn.Id, asset.Id, DateTime.UtcNow, null, null);

        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task GetGINs_ShouldReturnRecordedGINs()
    {
        using var db = TestContextFactory.CreateContext();

        var supplier = new Supplier { Name = "Apex Procurement Co." };
        db.Suppliers.Add(supplier);
        var po = new PurchasingOrder { OrderNumber = "PO-0001", OrderDate = DateTime.UtcNow, Supplier = supplier };
        db.PurchasingOrders.Add(po);

        var asset = new Asset { AssetCode = "AST-0099" };
        db.Assets.Add(asset);

        var grn = new GRN { GrnNumber = "GRN-0001", ReceivedDate = DateTime.UtcNow, PurchasingOrder = po, Asset = asset };
        db.GRNs.Add(grn);
        db.GINs.Add(new GIN { GinNumber = "GIN-0001", AssignedDate = DateTime.UtcNow, Condition = "Good", GRN = grn, Asset = asset });
        await db.SaveChangesAsync();

        var handler = new GetGINsQueryHandler(db);
        var result = await handler.Handle(new GetGINsQuery(), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("GIN-0001", result[0].GinNumber);
        Assert.Equal("GRN-0001", result[0].GrnNumber);
    }
}
