using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using Assura.Domain.Enums;
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
        stats.TotalUsers = await _context.Users.CountAsync(cancellationToken);

        // Assets by Division - Ensure all divisions from Seed are present
        var divisions = await _context.Divisions.Select(d => d.Name).ToListAsync(cancellationToken);
        var assetsByDivisionRaw = await _context.Assets
            .GroupBy(a => a.Division.Name)
            .Select(g => new { Label = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        stats.AssetsByDivision = divisions.Select(name => new StatItemDto
        {
            Label = name,
            Count = assetsByDivisionRaw.FirstOrDefault(x => x.Label == name)?.Count ?? 0
        }).ToList();

        // Assets by Category - Ensure all categories from Seed are present
        var categories = await _context.Categories.Select(c => c.Name).ToListAsync(cancellationToken);
        var assetsByCategoryRaw = await _context.Assets
            .GroupBy(a => a.Category.Name)
            .Select(g => new { Label = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        stats.AssetsByCategory = categories.Select(name => new StatItemDto
        {
            Label = name,
            Count = assetsByCategoryRaw.FirstOrDefault(x => x.Label == name)?.Count ?? 0
        }).ToList();

        // Assets by Status - Ensure all statuses from the image are represented
        var assetsByStatusRaw = await _context.Assets
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var statuses = new[] { AssetStatus.InUse, AssetStatus.InStore, AssetStatus.UnderMaintenance, AssetStatus.Discarded };
        
        stats.AssetsByStatus = statuses.Select(status => new StatItemDto
        {
            Label = status switch
            {
                AssetStatus.InUse => "In Use",
                AssetStatus.InStore => "In Store",
                AssetStatus.UnderMaintenance => "Under Maintenance",
                AssetStatus.Discarded => "Discarded",
                _ => status.ToString()
            },
            Count = assetsByStatusRaw.FirstOrDefault(x => x.Status == status)?.Count ?? 0
        }).ToList();

        return stats;
    }
}
