using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Assets.Queries;

public record GetAssetsQuery : IRequest<List<AssetSummaryDto>>;

public class GetAssetsQueryHandler : IRequestHandler<GetAssetsQuery, List<AssetSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAssetsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AssetSummaryDto>> Handle(GetAssetsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Assets
            .AsNoTracking()
            .Select(a => new AssetSummaryDto
            {
                Id = a.Id,
                AssetCode = a.AssetCode,
                AssetName = a.Product.Name // Using Product Name as Asset Name
            })
            .ToListAsync(cancellationToken);
    }
}
