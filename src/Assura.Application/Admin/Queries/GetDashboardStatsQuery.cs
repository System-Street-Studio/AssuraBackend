using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Admin.Queries;

public record GetDashboardStatsQuery : IRequest<DashboardStatsDto>;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly IApplicationDbContext _context;

    public GetDashboardStatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var stats = new DashboardStatsDto();

        stats.TotalAssets = await _context.Assets.CountAsync(cancellationToken);

        stats.AssetsByDivision = await _context.Assets
            .GroupBy(a => a.Division.Name)
            .Select(g => new StatItemDto
            {
                Label = g.Key,
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        stats.AssetsByCategory = await _context.Assets
            .GroupBy(a => a.Category.Name)
            .Select(g => new StatItemDto
            {
                Label = g.Key,
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        var assetsByStatusRaw = await _context.Assets
            .GroupBy(a => a.Status)
            .Select(g => new
            {
                Status = g.Key,
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        stats.AssetsByStatus = assetsByStatusRaw
            .Select(x => new StatItemDto
            {
                Label = x.Status.ToString(),
                Count = x.Count
            })
            .ToList();

        return stats;
    }
}
