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

    [Fact]
    public async Task GetAvailableAssetsForCheckout_ShouldIncludeAssetWithZeroStatusWhenUnassigned()
    {
        using var db = TestContextFactory.CreateContext();

        var product = new Product { Name = "Barcode Scanner" };
        var category = new Category { Name = "Electronics" };
        db.Products.Add(product);
        db.Categories.Add(category);

        var zeroStatusAsset = new Asset
        {
            AssetCode = "AST-ZERO-STATUS",
            Product = product,
            Category = category,
            Status = (AssetStatus)0,
            AssignedUserId = null,
            SerialNumber = "SN-ZERO-1"
        };
        db.Assets.Add(zeroStatusAsset);
        await db.SaveChangesAsync();

        var handler = new GetAvailableAssetsForCheckoutQueryHandler(db);
        var result = await handler.Handle(new GetAvailableAssetsForCheckoutQuery(), CancellationToken.None);

        Assert.Contains(result, a => a.AssetCode == "AST-ZERO-STATUS");
    }

    [Fact]
    public async Task CheckoutAssetCommand_ShouldAllowCheckoutOfAssetWithZeroStatus()
    {
        using var db = TestContextFactory.CreateContext();

        var product = new Product { Name = "Network Switch" };
        var category = new Category { Name = "Network" };
        var division = new Division { Name = "IT" };
        db.Products.Add(product);
        db.Categories.Add(category);
        db.Divisions.Add(division);

        var assignee = new User { Username = "network_emp", FirstName = "Network", LastName = "User", Email = "net@assura.com", Division = division, IsActive = true };
        db.Users.Add(assignee);

        var zeroStatusAsset = new Asset
        {
            AssetCode = "AST-NET-01",
            Product = product,
            Category = category,
            Status = (AssetStatus)0,
            AssignedUserId = null
        };
        db.Assets.Add(zeroStatusAsset);
        await db.SaveChangesAsync();

        var handler = new CheckoutAssetCommandHandler(db);
        var command = new CheckoutAssetCommand(zeroStatusAsset.Id, assignee.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)), "Checkout test", "Storekeeper");

        var result = await handler.Handle(command, CancellationToken.None);
        Assert.NotNull(result);

        var updated = await db.Assets.FindAsync(zeroStatusAsset.Id);
        Assert.NotNull(updated);
        Assert.Equal(AssetStatus.InUse, updated.Status);
        Assert.Equal(assignee.Id, updated.AssignedUserId);
    }

    [Fact]
    public async Task CreateAssetCommand_WithZeroStatus_ShouldDefaultToInStore()
    {
        using var db = TestContextFactory.CreateContext();

        var product = new Product { Name = "Monitor 27 inch" };
        var category = new Category { Name = "Electronics" };
        var division = new Division { Name = "Operations" };
        var supplier = new Supplier { Name = "Screen Tech" };
        db.Products.Add(product);
        db.Categories.Add(category);
        db.Divisions.Add(division);
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync();

        var dto = new DTOs.AssetCreateDto
        {
            AssetCode = "AST-MON-01",
            ProductId = product.Id,
            CategoryId = category.Id,
            DivisionId = division.Id,
            SupplierId = supplier.Id,
            PurchaseValue = 45000,
            Status = (AssetStatus)0,
            AssignedUserId = null
        };

        var handler = new CreateAssetCommandHandler(db);
        var result = await handler.Handle(new CreateAssetCommand(dto), CancellationToken.None);

        Assert.NotNull(result);
        var created = await db.Assets.FindAsync(result.Id);
        Assert.NotNull(created);
        Assert.Equal(AssetStatus.InStore, created.Status);
    }

    [Fact]
    public async Task CheckoutAssetCommand_ShouldAllowCheckout_WhenAssetIsEarmarkedForAssignee()
    {
        using var db = TestContextFactory.CreateContext();

        var product = new Product { Name = "Samsung A15" };
        var category = new Category { Name = "Mobile" };
        var division = new Division { Name = "Sales" };
        db.Products.Add(product);
        db.Categories.Add(category);
        db.Divisions.Add(division);

        var employee = new User
        {
            Username = "emp_target",
            FirstName = "Target",
            LastName = "Employee",
            Email = "target@assura.com",
            Division = division,
            IsActive = true
        };
        db.Users.Add(employee);
        await db.SaveChangesAsync();

        // Asset arrived from PO and is marked InStore with AssignedUserId tentatively matching the target employee
        var asset = new Asset
        {
            AssetCode = "AST-20260909-3970",
            Product = product,
            Category = category,
            Status = AssetStatus.InStore,
            AssignedUserId = employee.Id,
        };
        db.Assets.Add(asset);
        await db.SaveChangesAsync();

        var handler = new CheckoutAssetCommandHandler(db);
        var command = new CheckoutAssetCommand(asset.Id, employee.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)), null, null);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        var updated = await db.Assets.FindAsync(asset.Id);
        Assert.NotNull(updated);
        Assert.Equal(AssetStatus.InUse, updated.Status);
        Assert.Equal(employee.Id, updated.AssignedUserId);
    }
}
