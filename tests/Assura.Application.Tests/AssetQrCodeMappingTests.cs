using System;
using System.Threading;
using System.Threading.Tasks;
using Assura.Application.Features.Assets.Queries;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using Xunit;

namespace Assura.Application.Tests;

// Covers the BUGS.md finding: CreateAssetCommand generates and persists a real
// QR image into Asset.QrCode, but the read paths the mobile app and web
// frontend actually use to list/fetch assets afterward — GetAssetsQuery and
// GetAssetByIdQuery — never mapped it into AssetDto, so it always came back
// null regardless of what was actually stored.
public class AssetQrCodeMappingTests
{
    [Fact]
    public async Task GetAssetsQuery_IncludesPersistedQrCode()
    {
        using var db = TestContextFactory.CreateContext();
        SeedAsset(db);
        await db.SaveChangesAsync();

        var handler = new GetAssetsQueryHandler(db);
        var result = await handler.Handle(new GetAssetsQuery(), CancellationToken.None);

        var asset = Assert.Single(result);
        Assert.Equal("fake-qr-base64", asset.QrCode);
    }

    [Fact]
    public async Task GetAssetByIdQuery_IncludesPersistedQrCode()
    {
        using var db = TestContextFactory.CreateContext();
        SeedAsset(db);
        await db.SaveChangesAsync();

        var handler = new GetAssetByIdQueryHandler(db);
        var result = await handler.Handle(new GetAssetByIdQuery(1), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("fake-qr-base64", result!.QrCode);
    }

    private static void SeedAsset(TestApplicationDbContext db)
    {
        var category = new Category { Id = 1, Name = "Computers" };
        var product = new Product { Id = 1, Name = "Dell XPS 15" };
        var division = new Division { Id = 1, Name = "Engineering" };
        var supplier = new Supplier { Id = 1, Name = "Acme Supplies" };

        db.Categories.Add(category);
        db.Products.Add(product);
        db.Divisions.Add(division);
        db.Suppliers.Add(supplier);

        db.Assets.Add(new Asset
        {
            Id = 1,
            AssetCode = "AST-QR-001",
            AssetDate = DateTime.UtcNow,
            Status = AssetStatus.InStore,
            PurchaseValue = 100m,
            QrCode = "fake-qr-base64",
            CategoryId = category.Id,
            Category = category,
            ProductId = product.Id,
            Product = product,
            DivisionId = division.Id,
            Division = division,
            SupplierId = supplier.Id,
            Supplier = supplier,
        });
    }
}
