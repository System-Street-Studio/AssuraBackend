using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.NewArrivals.Queries;

public record GetAssetInformingsQuery : IRequest<List<AssetInformingDto>>;

public class GetAssetInformingsQueryHandler : IRequestHandler<GetAssetInformingsQuery, List<AssetInformingDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAssetInformingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AssetInformingDto>> Handle(GetAssetInformingsQuery request, CancellationToken cancellationToken)
    {
        return await _context.AssetInformings
            .Include(x => x.Division)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new AssetInformingDto
            {
                Id = x.Id,
                ItemName = x.ItemName,
                Model = x.Model,
                Warranty = x.Warranty,
                Quantity = x.Quantity,
                PurchasedDate = x.PurchasedDate,
                PurchasedPrice = x.PurchasedPrice,
                Status = x.Status,
                DivisionId = x.DivisionId,
                DivisionName = x.Division.Name,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
