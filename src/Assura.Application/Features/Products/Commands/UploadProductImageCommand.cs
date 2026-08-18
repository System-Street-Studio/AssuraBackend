using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Products.Commands;

public record UploadProductImageCommand(int Id, string ImageUrl) : IRequest<ProductDto?>;

public class UploadProductImageCommandHandler : IRequestHandler<UploadProductImageCommand, ProductDto?>
{
    private readonly IApplicationDbContext _context;

    public UploadProductImageCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDto?> Handle(UploadProductImageCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (entity == null) return null;

        entity.ImageUrl = request.ImageUrl;
        await _context.SaveChangesAsync(cancellationToken);

        return new ProductDto
        {
            Id = entity.Id,
            Name = entity.Name,
            ModelNumber = entity.ModelNumber,
            Manufacturer = entity.Manufacturer,
            Description = entity.Description,
            ImageUrl = entity.ImageUrl
        };
    }
}
