using Assura.Application.Features.Assets.Commands;
using Assura.Application.Features.Assets.Queries;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using FluentValidation;

namespace Assura.Application.Tests;

// Covers the Storekeeper end-to-end simulation finding: assets listed as
// "available for checkout" could still fail to actually check out, because
// GetAvailableAssetsForCheckoutQuery didn't apply the same eligibility rule
// as CheckoutAssetCommandHandler (Status == InStore && AssignedUserId == null).
public class CheckoutAvailabilityTests
{
    [Fact]
    public async Task GetAvailableAssetsForCheckout_ShouldExcludeAssetsWithStaleAssignedUser()
    {
        using var db = TestContextFactory.CreateContext();

        var product = new Product { Name = "Office Chair Ergonomic" };
        var category = new Category { Name = "Furniture & Fittings" };
        var division = new Division { Name = "Admin" };
        var supplier = new Supplier { Name = "Apex Procurement Co." };
        db.Products.Add(product);
        db.Categories.Add(category);
        db.Divisions.Add(division);
        db.Suppliers.Add(supplier);

        var genuinelyAvailable = new Asset
        {
            AssetCode = "AST-GOOD",
            Product = product,
            Category = category,
            Division = division,
            Supplier = supplier,
            Status = AssetStatus.InStore,
            AssignedUserId = null,
        };

        // Data-integrity edge case reproduced from the live DB: Status says
        // InStore but AssignedUserId is still set from a prior assignment.
        var staleAssignment = new Asset
        {
            AssetCode = "AST-STALE",
            Product = product,
            Category = category,
            Division = division,
            Supplier = supplier,
            Status = AssetStatus.InStore,
            AssignedUserId = 64,
        };

        db.Assets.AddRange(genuinelyAvailable, staleAssignment);
        await db.SaveChangesAsync();

        var handler = new GetAvailableAssetsForCheckoutQueryHandler(db);
        var result = await handler.Handle(new GetAvailableAssetsForCheckoutQuery(), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("AST-GOOD", result[0].AssetCode);
    }

    [Fact]
    public async Task CheckoutAssetCommand_ShouldRejectAssetWithStaleAssignedUser()
    {
        using var db = TestContextFactory.CreateContext();

        var product = new Product { Name = "Office Chair Ergonomic" };
        var category = new Category { Name = "Furniture & Fittings" };
        var division = new Division { Name = "Stores" };
        db.Products.Add(product);
        db.Categories.Add(category);
        db.Divisions.Add(division);

        var assignee = new User { Username = "emp_stores", FirstName = "Stores", LastName = "Employee", Email = "emp_stores@assura.com", Division = division, IsActive = true };
        db.Users.Add(assignee);

        var staleAsset = new Asset
        {
            AssetCode = "AST-STALE",
            Product = product,
            Category = category,
            Status = AssetStatus.InStore,
            AssignedUserId = 999,
        };
        db.Assets.Add(staleAsset);
        await db.SaveChangesAsync();

        var handler = new CheckoutAssetCommandHandler(db);
        var command = new CheckoutAssetCommand(staleAsset.Id, assignee.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)), null, null);

        // This is exactly the mismatch the availability query fix closes: any
        // asset the fixed query still offers must be checkout-able here too.
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
    }
}
