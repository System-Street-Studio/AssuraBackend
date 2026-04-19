using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Products.Commands;

public record UpdateProductCommand(ProductUpdateDto Product) : IRequest<ProductDto?>;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto?>
{
    private readonly IApplicationDbContext _context;

    public UpdateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDto?> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Product.Id, cancellationToken);

        if (entity == null) return null;

        entity.Name = request.Product.Name.Trim();
        entity.ModelNumber = string.IsNullOrWhiteSpace(request.Product.ModelNumber) ? null : request.Product.ModelNumber.Trim();
        entity.Manufacturer = string.IsNullOrWhiteSpace(request.Product.Manufacturer) ? null : request.Product.Manufacturer.Trim();
        entity.Description = string.IsNullOrWhiteSpace(request.Product.Description) ? null : request.Product.Description.Trim();

        await _context.SaveChangesAsync(cancellationToken);

        return new ProductDto
        {
            Id = entity.Id,
            Name = entity.Name,
            ModelNumber = entity.ModelNumber,
            Manufacturer = entity.Manufacturer,
            Description = entity.Description
        };
    }
}
