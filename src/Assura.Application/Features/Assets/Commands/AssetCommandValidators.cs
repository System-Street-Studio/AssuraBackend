using Assura.Application.Common.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Assets.Commands;

/// <summary>
/// Validates asset creation. The database already carries a unique index on
/// <c>Asset.AssetCode</c>, so without these rules a duplicate code surfaced only as a
/// raw <c>DbUpdateException</c> (HTTP 500). Validating here turns it into a 400 with a
/// message the client can display against the field.
/// </summary>
public class CreateAssetCommandValidator : AbstractValidator<CreateAssetCommand>
{
    public CreateAssetCommandValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.Asset.AssetCode)
            .MaximumLength(50).WithMessage("Asset code cannot be longer than 50 characters.");

        RuleFor(x => x)
            .MustAsync((cmd, ct) => BeUniqueAssetCode(context, cmd.Asset.AssetCode, null, ct))
            .WithMessage(cmd => $"Asset code '{(cmd.Asset.AssetCode ?? string.Empty).Trim()}' already exists. Please use a different code.");

        RuleFor(x => x.Asset.PurchaseValue)
            .GreaterThanOrEqualTo(0).WithMessage("Purchase value cannot be negative.");
    }

    internal static async Task<bool> BeUniqueAssetCode(
        IApplicationDbContext context,
        string? assetCode,
        int? ignoreId,
        CancellationToken cancellationToken)
    {
        var code = (assetCode ?? string.Empty).Trim();

        // An empty code is allowed: the create handler generates one. Nothing to collide with yet.
        if (string.IsNullOrEmpty(code)) return true;

        // IgnoreQueryFilters is deliberate. The unique index spans soft-deleted rows as well,
        // so a code still held by a soft-deleted asset would pass this check and then fail at
        // SaveChangesAsync. Comparison is left to the database so it matches the index's
        // collation rather than second-guessing it.
        return !await context.Assets
            .IgnoreQueryFilters()
            .AnyAsync(a => (ignoreId == null || a.Id != ignoreId.Value) && a.AssetCode == code, cancellationToken);
    }
}

/// <summary>
/// Validates asset updates. Mirrors <see cref="CreateAssetCommandValidator"/> but excludes the
/// asset being edited from the uniqueness check, so re-saving an unchanged code is allowed.
/// </summary>
public class UpdateAssetCommandValidator : AbstractValidator<UpdateAssetCommand>
{
    public UpdateAssetCommandValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.Asset.Id)
            .GreaterThan(0).WithMessage("A valid asset id is required.");

        RuleFor(x => x.Asset.AssetCode)
            .NotEmpty().WithMessage("Asset code is required.")
            .MaximumLength(50).WithMessage("Asset code cannot be longer than 50 characters.");

        RuleFor(x => x)
            .MustAsync((cmd, ct) => CreateAssetCommandValidator.BeUniqueAssetCode(context, cmd.Asset.AssetCode, cmd.Asset.Id, ct))
            .WithMessage(cmd => $"Asset code '{(cmd.Asset.AssetCode ?? string.Empty).Trim()}' already exists. Please use a different code.");

        RuleFor(x => x.Asset.PurchaseValue)
            .GreaterThanOrEqualTo(0).WithMessage("Purchase value cannot be negative.");
    }
}
