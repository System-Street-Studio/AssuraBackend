using Assura.Application.DTOs;
using Assura.Application.Features.Assets.Commands;
using Assura.Application.Features.GRNs.Commands;
using Assura.Application.NewArrivals.Queries;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using Xunit;

namespace Assura.Application.Tests;

public class ArrivalToCheckoutWorkflowTests
{
    [Fact]
    public async Task CreateAsset_WithInformingId_ShouldUpdateInformingStatusToGRNRecordedAndLinkAsset()
    {
        using var db = TestContextFactory.CreateContext();

        var division = new Division { Name = "IT Department" };
        var category = new Category { Name = "Computers" };
        var supplier = new Supplier { Name = "Tech Distributor Ltd" };

        db.Divisions.Add(division);
        db.Categories.Add(category);
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync();

        var product = new Product { Name = "All-in-one PC" };
        db.Products.Add(product);

        var po = new PurchasingOrder { OrderNumber = "PO-2026-0005", Supplier = supplier, Division = division };
        db.PurchasingOrders.Add(po);

        var informing = new AssetInforming
        {
            ItemName = "All-in-one PC",
            Model = "AIO-5000",
            Status = "Confirmed",
            Quantity = 1,
            PurchasedPrice = 150000,
            Division = division,
            PurchasingOrder = po
        };
        db.AssetInformings.Add(informing);
        await db.SaveChangesAsync();

        var dto = new AssetCreateDto
        {
            AssetCode = "AST-20260820-0001",
            AssetTag = "TAG-AIO-01",
            ProductId = product.Id,
            CategoryId = category.Id,
            SupplierId = supplier.Id,
            DivisionId = division.Id,
            Status = AssetStatus.InStore,
            SerialNumber = "SN-AIO-999",
            AssetDate = DateTime.UtcNow,
            PurchaseValue = 150000,
            Notes = "Registered from arrival",
            PurchasingOrderId = po.Id,
            InformingId = informing.Id
        };

        var handler = new CreateAssetCommandHandler(db);
        var result = await handler.Handle(new CreateAssetCommand(dto), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("AST-20260820-0001", result.AssetCode);

        // Verify Informing status updated to GRN Recorded and linked AssetId
        var updatedInforming = await db.AssetInformings.FindAsync(informing.Id);
        Assert.NotNull(updatedInforming);
        Assert.Equal("GRN Recorded", updatedInforming.Status);
        Assert.Equal(result.Id, updatedInforming.AssetId);
    }

    [Fact]
    public async Task CreateGRN_WithInformingIdAndZeroPO_ShouldSucceedAndTransitionStatus()
    {
        using var db = TestContextFactory.CreateContext();

        var division = new Division { Name = "Operations" };
        var category = new Category { Name = "Hardware" };
        var supplier = new Supplier { Name = "Global Tech" };

        db.Divisions.Add(division);
        db.Categories.Add(category);
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync();

        var product = new Product { Name = "Server Unit" };
        db.Products.Add(product);

        var asset = new Asset
        {
            AssetCode = "AST-SERVER-01",
            Product = product,
            Category = category,
            Supplier = supplier,
            Division = division,
            Status = AssetStatus.InStore,
            PurchaseValue = 350000
        };
        db.Assets.Add(asset);

        var informing = new AssetInforming
        {
            ItemName = "Server Unit",
            Model = "Rackmount 2U",
            Status = "Confirmed",
            Quantity = 1,
            PurchasedPrice = 350000,
            Division = division
        };
        db.AssetInformings.Add(informing);
        await db.SaveChangesAsync();

        var handler = new CreateGRNCommandHandler(db);
        var command = new CreateGRNCommand(
            PurchasingOrderId: 0,
            AssetId: asset.Id,
            ReceivedDate: DateTime.UtcNow,
            ReceivedBy: "Storekeeper John",
            Notes: "GRN for arrival without explicit PO",
            InformingId: informing.Id
        );

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.StartsWith("GRN-", result.GrnNumber);

        // Verify AssetInforming status is GRN Recorded
        var updatedInforming = await db.AssetInformings.FindAsync(informing.Id);
        Assert.NotNull(updatedInforming);
        Assert.Equal("GRN Recorded", updatedInforming.Status);
        Assert.Equal(asset.Id, updatedInforming.AssetId);
    }

    [Fact]
    public async Task GetAssetInformings_ShouldReturnLinkedAssetIdAndPurchasingOrderId()
    {
        using var db = TestContextFactory.CreateContext();

        var division = new Division { Name = "Finance" };
        var supplier = new Supplier { Name = "Office Depot" };
        var po = new PurchasingOrder { OrderNumber = "PO-9999", Supplier = supplier, Division = division };
        db.Divisions.Add(division);
        db.Suppliers.Add(supplier);
        db.PurchasingOrders.Add(po);

        var asset = new Asset { AssetCode = "AST-LAPTOP-05" };
        db.Assets.Add(asset);
        await db.SaveChangesAsync();

        var informing = new AssetInforming
        {
            ItemName = "High Performance Laptop",
            Status = "GRN Recorded",
            Quantity = 1,
            Division = division,
            PurchasingOrder = po,
            AssetId = asset.Id
        };
        db.AssetInformings.Add(informing);
        await db.SaveChangesAsync();

        var handler = new GetAssetInformingsQueryHandler(db);
        var result = await handler.Handle(new GetAssetInformingsQuery(), CancellationToken.None);

        Assert.NotEmpty(result);
        var dto = result.FirstOrDefault(x => x.Id == informing.Id);
        Assert.NotNull(dto);
        Assert.Equal("GRN Recorded", dto.Status);
        Assert.Equal(asset.Id, dto.AssetId);
        Assert.Equal("AST-LAPTOP-05", dto.AssetCode);
        Assert.Equal(po.Id, dto.PurchasingOrderId);
    }
}
