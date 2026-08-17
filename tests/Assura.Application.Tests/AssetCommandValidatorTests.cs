using Assura.Application.DTOs;
using Assura.Application.Features.Assets.Commands;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;

namespace Assura.Application.Tests;

// The database carries a unique index on Asset.AssetCode, but nothing in the application
// layer checked it, so creating an asset with a code already in use surfaced as a raw
// DbUpdateException (HTTP 500) instead of a validation error the storekeeper could act on.
// Negative purchase values were likewise accepted by the API even though the form rejects them.
public class AssetCommandValidatorTests
{
    private static AssetCreateDto NewAssetDto(string assetCode, decimal purchaseValue = 100m) => new()
    {
        AssetCode = assetCode,
        AssetDate = DateTime.UtcNow,
        PurchaseValue = purchaseValue,
    };

    [Fact]
    public async Task CreateAsset_WithDuplicateAssetCode_ShouldFailValidation()
    {
        using var db = TestContextFactory.CreateContext();
        db.Assets.Add(new Asset { AssetCode = "AST-20260817-1234" });
        await db.SaveChangesAsync();

        var validator = new CreateAssetCommandValidator(db);

        var result = await validator.ValidateAsync(
            new CreateAssetCommand(NewAssetDto("AST-20260817-1234")));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("already exists"));
    }

    [Fact]
    public async Task CreateAsset_WithDuplicateAssetCodeDifferingByWhitespace_ShouldFailValidation()
    {
        using var db = TestContextFactory.CreateContext();
        db.Assets.Add(new Asset { AssetCode = "AST-20260817-1234" });
        await db.SaveChangesAsync();

        var validator = new CreateAssetCommandValidator(db);

        var result = await validator.ValidateAsync(
            new CreateAssetCommand(NewAssetDto("  AST-20260817-1234  ")));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("already exists"));
    }

    [Fact]
    public async Task CreateAsset_WithUnusedAssetCode_ShouldPassValidation()
    {
        using var db = TestContextFactory.CreateContext();
        db.Assets.Add(new Asset { AssetCode = "AST-20260817-1234" });
        await db.SaveChangesAsync();

        var validator = new CreateAssetCommandValidator(db);

        var result = await validator.ValidateAsync(
            new CreateAssetCommand(NewAssetDto("AST-20260817-9999")));

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task CreateAsset_WithEmptyAssetCode_ShouldPassValidation()
    {
        // An empty code is legitimate: the create handler generates one.
        using var db = TestContextFactory.CreateContext();
        var validator = new CreateAssetCommandValidator(db);

        var result = await validator.ValidateAsync(new CreateAssetCommand(NewAssetDto("")));

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task CreateAsset_WithNegativePurchaseValue_ShouldFailValidation()
    {
        using var db = TestContextFactory.CreateContext();
        var validator = new CreateAssetCommandValidator(db);

        var result = await validator.ValidateAsync(
            new CreateAssetCommand(NewAssetDto("AST-20260817-5555", purchaseValue: -1m)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("cannot be negative"));
    }

    [Fact]
    public async Task CreateAsset_WithZeroPurchaseValue_ShouldPassValidation()
    {
        // Zero is allowed — donated and written-down assets legitimately have no cost.
        using var db = TestContextFactory.CreateContext();
        var validator = new CreateAssetCommandValidator(db);

        var result = await validator.ValidateAsync(
            new CreateAssetCommand(NewAssetDto("AST-20260817-5556", purchaseValue: 0m)));

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task CreateAsset_WithAssetCodeOver50Characters_ShouldFailValidation()
    {
        using var db = TestContextFactory.CreateContext();
        var validator = new CreateAssetCommandValidator(db);

        var result = await validator.ValidateAsync(
            new CreateAssetCommand(NewAssetDto(new string('A', 51))));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("50 characters"));
    }

    [Fact]
    public async Task CreateAsset_WithCodeHeldBySoftDeletedAsset_ShouldFailValidation()
    {
        // The unique index spans soft-deleted rows, so this must be rejected up front
        // rather than blowing up at SaveChangesAsync.
        using var db = TestContextFactory.CreateContext();
        db.Assets.Add(new Asset { AssetCode = "AST-20260817-7777", IsDeleted = true });
        await db.SaveChangesAsync();

        var validator = new CreateAssetCommandValidator(db);

        var result = await validator.ValidateAsync(
            new CreateAssetCommand(NewAssetDto("AST-20260817-7777")));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("already exists"));
    }

    [Fact]
    public async Task UpdateAsset_KeepingItsOwnAssetCode_ShouldPassValidation()
    {
        using var db = TestContextFactory.CreateContext();
        var asset = new Asset { AssetCode = "AST-20260817-1234" };
        db.Assets.Add(asset);
        await db.SaveChangesAsync();

        var validator = new UpdateAssetCommandValidator(db);

        var result = await validator.ValidateAsync(new UpdateAssetCommand(new AssetUpdateDto
        {
            Id = asset.Id,
            AssetCode = "AST-20260817-1234",
            AssetDate = DateTime.UtcNow,
            PurchaseValue = 250m,
        }));

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task UpdateAsset_TakingAnotherAssetsCode_ShouldFailValidation()
    {
        using var db = TestContextFactory.CreateContext();
        var first = new Asset { AssetCode = "AST-20260817-1111" };
        var second = new Asset { AssetCode = "AST-20260817-2222" };
        db.Assets.AddRange(first, second);
        await db.SaveChangesAsync();

        var validator = new UpdateAssetCommandValidator(db);

        var result = await validator.ValidateAsync(new UpdateAssetCommand(new AssetUpdateDto
        {
            Id = second.Id,
            AssetCode = "AST-20260817-1111",
            AssetDate = DateTime.UtcNow,
            PurchaseValue = 250m,
        }));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("already exists"));
    }

    [Fact]
    public async Task UpdateAsset_WithEmptyAssetCode_ShouldFailValidation()
    {
        // Unlike create, an update must not blank out an existing code.
        using var db = TestContextFactory.CreateContext();
        var asset = new Asset { AssetCode = "AST-20260817-1234" };
        db.Assets.Add(asset);
        await db.SaveChangesAsync();

        var validator = new UpdateAssetCommandValidator(db);

        var result = await validator.ValidateAsync(new UpdateAssetCommand(new AssetUpdateDto
        {
            Id = asset.Id,
            AssetCode = "",
            AssetDate = DateTime.UtcNow,
            PurchaseValue = 250m,
        }));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("required"));
    }
}
