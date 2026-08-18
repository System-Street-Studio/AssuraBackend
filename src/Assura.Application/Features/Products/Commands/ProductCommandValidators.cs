using Assura.Application.Common.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Products.Commands;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.Product.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(120);

        RuleFor(x => x.Product.Manufacturer)
            .NotEmpty().WithMessage("Manufacturer is required.")
            .MaximumLength(120);

        RuleFor(x => x.Product.ModelNumber)
            .MaximumLength(120);

        RuleFor(x => x.Product.Description)
            .MaximumLength(500);

        RuleFor(x => x)
            .MustAsync((cmd, ct) => BeUniqueProduct(context, cmd.Product.Name, cmd.Product.Manufacturer, cmd.Product.ModelNumber, null, ct))
            .WithMessage("A product with the same name, manufacturer, and model already exists.");
    }

    private static async Task<bool> BeUniqueProduct(
        IApplicationDbContext context,
        string name,
        string? manufacturer,
        string? modelNumber,
        int? ignoreId,
        CancellationToken cancellationToken)
    {
        var n = Normalize(name);
        var m = Normalize(manufacturer);
        var model = Normalize(modelNumber);

        return !await context.Products.AnyAsync(p =>
            (ignoreId == null || p.Id != ignoreId.Value) &&
            p.Name.ToLower() == n &&
            (p.Manufacturer ?? string.Empty).ToLower() == m &&
            (p.ModelNumber ?? string.Empty).ToLower() == model,
            cancellationToken);
    }

    private static string Normalize(string? value)
    {
        return (value ?? string.Empty).Trim().ToLowerInvariant();
    }
}

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.Product.Id)
            .GreaterThan(0);

        RuleFor(x => x.Product.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(120);

        RuleFor(x => x.Product.Manufacturer)
            .NotEmpty().WithMessage("Manufacturer is required.")
            .MaximumLength(120);

        RuleFor(x => x.Product.ModelNumber)
            .MaximumLength(120);

        RuleFor(x => x.Product.Description)
            .MaximumLength(500);

        RuleFor(x => x)
            .MustAsync((cmd, ct) => BeUniqueProduct(context, cmd.Product.Name, cmd.Product.Manufacturer, cmd.Product.ModelNumber, cmd.Product.Id, ct))
            .WithMessage("A product with the same name, manufacturer, and model already exists.");
    }

    private static async Task<bool> BeUniqueProduct(
        IApplicationDbContext context,
        string name,
        string? manufacturer,
        string? modelNumber,
        int ignoreId,
        CancellationToken cancellationToken)
    {
        var n = Normalize(name);
        var m = Normalize(manufacturer);
        var model = Normalize(modelNumber);

        return !await context.Products.AnyAsync(p =>
            p.Id != ignoreId &&
            p.Name.ToLower() == n &&
            (p.Manufacturer ?? string.Empty).ToLower() == m &&
            (p.ModelNumber ?? string.Empty).ToLower() == model,
            cancellationToken);
    }

    private static string Normalize(string? value)
    {
        return (value ?? string.Empty).Trim().ToLowerInvariant();
    }
}
