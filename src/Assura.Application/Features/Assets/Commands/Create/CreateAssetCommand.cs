using MediatR;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Assets.Commands.Create;

public class CreateAssetCommand : IRequest<string>
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;
    public string Status { get; set; } = "InUse";
}

public class CreateAssetCommandHandler : IRequestHandler<CreateAssetCommand, string>
{
    private readonly IApplicationDbContext _context;

    public CreateAssetCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> Handle(CreateAssetCommand request, CancellationToken cancellationToken)
    {
        // Find or create Category
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == request.Type, cancellationToken);
        if (category == null)
        {
            category = new Category { Name = request.Type };
            _context.Categories.Add(category);
        }

        // Find or create Division
        var division = await _context.Divisions.FirstOrDefaultAsync(d => d.Name == request.Division, cancellationToken);
        if (division == null)
        {
            division = new Division { Name = request.Division };
            _context.Divisions.Add(division);
        }

        // Find or create Product
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Name == request.Name, cancellationToken);
        if (product == null)
        {
            product = new Product { Name = request.Name };
            _context.Products.Add(product);
        }

        // Find or create a default Supplier
        var supplier = await _context.Suppliers.FirstOrDefaultAsync(cancellationToken);
        if (supplier == null)
        {
            supplier = new Supplier { Name = "Default Supplier" };
            _context.Suppliers.Add(supplier);
        }

        // Parse status safely (default to InUse)
        AssetStatus assetStatus = AssetStatus.InUse;
        Enum.TryParse(request.Status, true, out assetStatus);

        var asset = new Asset
        {
            AssetCode = "AST-" + Guid.NewGuid().ToString().Substring(0, 6).ToUpper(),
            SerialNumber = request.SerialNumber,
            Status = assetStatus,
            AssetDate = DateTime.UtcNow,
            PurchaseValue = 0,
            Category = category,
            Division = division,
            Product = product,
            Supplier = supplier
        };

        _context.Assets.Add(asset);
        await _context.SaveChangesAsync(cancellationToken);

        return asset.Id.ToString();
    }
}
