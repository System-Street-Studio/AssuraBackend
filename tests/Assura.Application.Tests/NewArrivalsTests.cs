using Assura.Application.DTOs;
using Assura.Application.NewArrivals.Commands;
using Assura.Application.NewArrivals.Queries;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Tests;

public class NewArrivalsTests
{
    [Fact]
    public async Task GetAssetInformings_ShouldReturnHistory()
    {
        using var db = CreateContext();
        
        var division = new Division { Name = "Stores" };
        db.Divisions.Add(division);
        db.AssetInformings.Add(new AssetInforming 
        { 
            ItemName = "New Laptop", 
            Division = division,
            CreatedAt = DateTime.UtcNow 
        });
        await db.SaveChangesAsync();

        var handler = new GetAssetInformingsQueryHandler(db);
        var result = await handler.Handle(new GetAssetInformingsQuery(), CancellationToken.None);

        Assert.NotEmpty(result);
        Assert.Equal("New Laptop", result[0].ItemName);
    }

    [Fact]
    public async Task InformStores_ShouldCreateInformingRecordAndNotifyStorekeepers()
    {
        using var db = CreateContext();
        
        var storekeeper = new User { Id = 1, Username = "sk", Role = UserRole.Storekeeper };
        db.Users.Add(storekeeper);
        await db.SaveChangesAsync();

        var handler = new InformStoresCommandHandler(db);
        var dto = new InformStoresDto
        {
            ItemName = "Monitors",
            Quantity = 10,
            PurchasedPrice = 200000,
            DivisionId = 1
        };

        var id = await handler.Handle(new InformStoresCommand(dto), CancellationToken.None);

        var informing = await db.AssetInformings.FindAsync(id);
        Assert.NotNull(informing);
        Assert.Equal("Monitors", informing!.ItemName);

        var notification = await db.Notifications.FirstOrDefaultAsync(n => n.UserId == storekeeper.Id);
        Assert.NotNull(notification);
        Assert.Contains("Monitors", notification!.Message);
    }

    private static TestApplicationDbContext CreateContext() => TestContextFactory.CreateContext();
}
