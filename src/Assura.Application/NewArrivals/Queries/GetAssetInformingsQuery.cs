using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.NewArrivals.Queries;

public record GetAssetInformingsQuery : IRequest<List<AssetInforming>>;

public class GetAssetInformingsQueryHandler : IRequestHandler<GetAssetInformingsQuery, List<AssetInforming>>
{
    private readonly IApplicationDbContext _context;

    public GetAssetInformingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AssetInforming>> Handle(GetAssetInformingsQuery request, CancellationToken cancellationToken)
    {
        return await _context.AssetInformings
            .Include(x => x.Division)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
