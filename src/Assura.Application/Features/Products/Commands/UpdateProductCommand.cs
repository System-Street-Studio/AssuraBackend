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

        entity.Name = request.Product.Name;
        entity.ModelNumber = request.Product.ModelNumber;
        entity.Manufacturer = request.Product.Manufacturer;
        entity.Description = request.Product.Description;

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
