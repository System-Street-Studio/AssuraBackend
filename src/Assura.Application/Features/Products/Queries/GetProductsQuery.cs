using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Products.Queries;

public record GetProductsQuery : IRequest<List<ProductDto>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<ProductDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        // Newest first. Id is the auto-increment primary key, so the highest value is always
        // the most recently created row — unlike relying on unspecified physical storage order.
        return await _context.Products
            .AsNoTracking()
            .OrderByDescending(p => p.Id)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                ModelNumber = p.ModelNumber,
                Manufacturer = p.Manufacturer,
                Description = p.Description,
                ImageUrl = p.ImageUrl
            })
            .ToListAsync(cancellationToken);
    }
}
