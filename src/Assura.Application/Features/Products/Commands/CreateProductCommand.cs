using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using Assura.Domain.Entities;
using MediatR;

namespace Assura.Application.Features.Products.Commands;

public record CreateProductCommand(ProductCreateDto Product) : IRequest<ProductDto>;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IApplicationDbContext _context;

    public CreateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var entity = new Product
        {
            Name = request.Product.Name.Trim(),
            ModelNumber = string.IsNullOrWhiteSpace(request.Product.ModelNumber) ? null : request.Product.ModelNumber.Trim(),
            Manufacturer = string.IsNullOrWhiteSpace(request.Product.Manufacturer) ? null : request.Product.Manufacturer.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Product.Description) ? null : request.Product.Description.Trim()
        };

        _context.Products.Add(entity);
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
