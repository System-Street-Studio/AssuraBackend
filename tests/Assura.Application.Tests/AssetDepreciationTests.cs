using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Assura.Application.Features.Depreciation.Queries;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using Xunit;

namespace Assura.Application.Tests;

public class AssetDepreciationTests
{
    [Fact]
    public async Task StraightLineDepreciation_CalculatesAccurateCurrentValue_AndAccumulatedDepreciation()
    {
        using var db = CreateContext();

        var category = new Category { Id = 1, Name = "Computers", DepreciationRate = 20.0m };
        var product = new Product { Id = 1, Name = "Dell XPS 15" };
        var division = new Division { Id = 1, Name = "Engineering" };

        db.Categories.Add(category);
        db.Products.Add(product);
        db.Divisions.Add(division);

        var assetDate = DateTime.UtcNow.AddDays(-730.5); // Exactly 2 years ago

        db.Assets.Add(new Asset
        {
            Id = 1,
            AssetCode = "AST-COMP-001",
            PurchaseValue = 10000.00m,
            AssetDate = assetDate,
            CategoryId = category.Id,
            Category = category,
            ProductId = product.Id,
            Product = product,
            DivisionId = division.Id,
            Division = division,
            Status = AssetStatus.InUse
        });

        await db.SaveChangesAsync();

        var handler = new GetAssetDepreciationQueryHandler(db);
        var result = await handler.Handle(new GetAssetDepreciationQuery(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(1, result.TotalAssets);
        Assert.Equal(10000.00m, result.TotalPurchaseValue);

        var assetDto = Assert.Single(result.Assets);
        Assert.Equal(20.0m, assetDto.DepreciationRate);
        Assert.Equal(2000.00m, assetDto.AnnualDepreciation);
        // Approx 2 years -> 4000 depreciation, remaining ~6000
        Assert.InRange(assetDto.AccumulatedDepreciation, 3990.00m, 4010.00m);
        Assert.InRange(assetDto.CurrentValue, 5990.00m, 6010.00m);
        Assert.False(assetDto.IsFullyDepreciated);
    }

    [Fact]
    public async Task Depreciation_NeverFallsBelowZero_FloorEnforced()
    {
        using var db = CreateContext();

        var category = new Category { Id = 1, Name = "Vehicles", DepreciationRate = 25.0m };
        var product = new Product { Id = 1, Name = "Delivery Van" };

        db.Categories.Add(category);
        db.Products.Add(product);

        // Acquired 6 years ago (at 25%/yr, 100% is reached in 4 years)
        var oldAssetDate = DateTime.UtcNow.AddYears(-6);

        db.Assets.Add(new Asset
        {
            Id = 2,
            AssetCode = "AST-VEH-002",
            PurchaseValue = 30000.00m,
            AssetDate = oldAssetDate,
            CategoryId = category.Id,
            Category = category,
            ProductId = product.Id,
            Product = product,
            Status = AssetStatus.InUse
        });

        await db.SaveChangesAsync();

        var handler = new GetAssetDepreciationQueryHandler(db);
        var result = await handler.Handle(new GetAssetDepreciationQuery(), CancellationToken.None);

        var assetDto = Assert.Single(result.Assets);
        Assert.Equal(0.00m, assetDto.CurrentValue); // Strictly $0.00 floor
        Assert.Equal(30000.00m, assetDto.AccumulatedDepreciation); // Cannot exceed purchase price
        Assert.True(assetDto.IsFullyDepreciated);
        Assert.Equal(1, result.FullyDepreciatedAssets);
        Assert.Equal(0, result.ActiveDepreciatingAssets);
        Assert.Equal(0.00m, result.TotalCurrentValue);
    }

    [Fact]
    public async Task MultiYearSchedule_GeneratesAccurateYearByYearDownToZero()
    {
        using var db = CreateContext();

        var category = new Category { Id = 1, Name = "Office Equipment", DepreciationRate = 33.33m };
        var product = new Product { Id = 1, Name = "High Speed Printer" };

        db.Categories.Add(category);
        db.Products.Add(product);

        var asset = new Asset
        {
            Id = 10,
            AssetCode = "AST-PRN-010",
            PurchaseValue = 1000.00m,
            AssetDate = new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CategoryId = category.Id,
            Category = category,
            ProductId = product.Id,
            Product = product,
            Status = AssetStatus.InUse
        };
        db.Assets.Add(asset);
        await db.SaveChangesAsync();

        var handler = new GetAssetDepreciationScheduleQueryHandler(db);
        var scheduleResult = await handler.Handle(new GetAssetDepreciationScheduleQuery(10), CancellationToken.None);

        Assert.NotNull(scheduleResult);
        Assert.NotEmpty(scheduleResult.Schedule);

        // Verify ending values never fall below zero
        foreach (var row in scheduleResult.Schedule)
        {
            Assert.True(row.EndingValue >= 0m, $"Ending value in year {row.YearNumber} must be >= 0");
            Assert.True(row.BeginningValue >= 0m, $"Beginning value in year {row.YearNumber} must be >= 0");
        }

        // Final row in schedule should end at $0.00
        var finalRow = scheduleResult.Schedule.Last();
        Assert.Equal(0.00m, finalRow.EndingValue);
    }

    [Fact]
    public async Task CategorySpecificDepreciationRates_ApplyProperlyToSummary()
    {
        using var db = CreateContext();

        var catBuilding = new Category { Id = 1, Name = "Buildings", DepreciationRate = 5.0m };
        var catFurniture = new Category { Id = 2, Name = "Furniture", DepreciationRate = 10.0m };

        db.Categories.AddRange(catBuilding, catFurniture);

        db.Assets.AddRange(
            new Asset
            {
                Id = 1,
                AssetCode = "AST-BLD-01",
                PurchaseValue = 500000m,
                AssetDate = DateTime.UtcNow.AddYears(-2),
                CategoryId = catBuilding.Id,
                Category = catBuilding,
                Status = AssetStatus.InUse
            },
            new Asset
            {
                Id = 2,
                AssetCode = "AST-FUR-01",
                PurchaseValue = 20000m,
                AssetDate = DateTime.UtcNow.AddYears(-1),
                CategoryId = catFurniture.Id,
                Category = catFurniture,
                Status = AssetStatus.InUse
            }
        );

        await db.SaveChangesAsync();

        var handler = new GetAssetDepreciationQueryHandler(db);
        var result = await handler.Handle(new GetAssetDepreciationQuery(), CancellationToken.None);

        Assert.Equal(2, result.CategoryBreakdown.Count);
        var bldSummary = result.CategoryBreakdown.First(c => c.CategoryId == catBuilding.Id);
        Assert.Equal(5.0m, bldSummary.DepreciationRate);
        Assert.Equal(500000m, bldSummary.TotalPurchaseValue);

        var furSummary = result.CategoryBreakdown.First(c => c.CategoryId == catFurniture.Id);
        Assert.Equal(10.0m, furSummary.DepreciationRate);
        Assert.Equal(20000m, furSummary.TotalPurchaseValue);
    }

    private static TestApplicationDbContext CreateContext() => TestContextFactory.CreateContext();
}
