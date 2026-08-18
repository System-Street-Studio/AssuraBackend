using Assura.Application.Admin.Queries;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Tests;

public class AdminTests
{
    [Fact]
    public async Task GetDashboardStats_ShouldReturnCorrectCountsPerCategoryAndStatus()
    {
        using var db = CreateContext();
        
        var cat1 = new Category { Name = "Laptops" };
        var cat2 = new Category { Name = "Printers" };
        db.Categories.AddRange(cat1, cat2);

        var div1 = new Division { Name = "IT" };
        db.Divisions.Add(div1);

        db.Assets.Add(new Asset { Status = AssetStatus.InUse, Category = cat1, Division = div1 });
        db.Assets.Add(new Asset { Status = AssetStatus.InStore, Category = cat1, Division = div1 });
        db.Assets.Add(new Asset { Status = AssetStatus.InUse, Category = cat2, Division = div1 });

        await db.SaveChangesAsync();

        var handler = new GetDashboardStatsQueryHandler(db);
        var stats = await handler.Handle(new GetDashboardStatsQuery(), CancellationToken.None);

        Assert.Equal(3, stats.TotalAssets);
        
        // Category check
        var laptopStat = stats.AssetsByCategory.FirstOrDefault(c => c.Label == "Laptops");
        Assert.NotNull(laptopStat);
        Assert.Equal(2, laptopStat!.Count);

        // Status check
        var inUseStat = stats.AssetsByStatus.FirstOrDefault(s => s.Label == "In Use");
        Assert.NotNull(inUseStat);
        Assert.Equal(2, inUseStat!.Count);

        var inStoreStat = stats.AssetsByStatus.FirstOrDefault(s => s.Label == "In Store");
        Assert.NotNull(inStoreStat);
        Assert.Equal(1, inStoreStat!.Count);
    }

    private static TestApplicationDbContext CreateContext() => TestContextFactory.CreateContext();
}
