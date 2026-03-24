using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assura.Application.Features.Dashboard.Queries;

public record GetDashboardQuery : IRequest<DashboardDto>;

public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    private readonly IApplicationDbContext _context;

    public GetDashboardQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var dashboard = new DashboardDto();

        // 1. KPIs
        var allAssets = await _context.Assets.AsNoTracking().ToListAsync(cancellationToken);
        dashboard.Kpis.TotalAssets = allAssets.Count;
        dashboard.Kpis.CheckedOut = allAssets.Count(a => a.Status == AssetStatus.InUse);
        dashboard.Kpis.Available = allAssets.Count(a => a.Status == AssetStatus.InStore);
        dashboard.Kpis.MaintenanceDue = allAssets.Count(a => a.Status == AssetStatus.UnderMaintenance);
        
        var totalValue = allAssets.Sum(a => a.PurchaseValue);
        dashboard.Kpis.TotalAssetValue = $"LKR {totalValue:N0}";

        dashboard.Kpis.PendingRequests = await _context.Requests
            .AsNoTracking()
            .CountAsync(r => r.IsDeleted == false, cancellationToken);

        // 2. Charts - Assets By Category
        var assetsByCategory = allAssets
            .GroupBy(a => a.CategoryId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToList();

        var categories = await _context.Categories.AsNoTracking().ToListAsync(cancellationToken);
        dashboard.Charts.AssetsByCategory.Labels = categories.Select(c => c.Name).ToList();
        dashboard.Charts.AssetsByCategory.Data = categories.Select(c => assetsByCategory.FirstOrDefault(x => x.Key == c.Id)?.Count ?? 0).ToList();
        dashboard.Charts.AssetsByCategory.Colors = new List<string> { "#0b6c78", "#ff8c42", "#3ecf8e", "#6366f1", "#7e7f86" };

        // 3. Charts - Assets By Status
        var statusGroups = allAssets.GroupBy(a => a.Status).Select(g => new { g.Key, Count = g.Count() }).ToList();
        dashboard.Charts.AssetsByStatus.Labels = new List<string> { "In Use", "Available", "Maintenance", "Discarded" };
        dashboard.Charts.AssetsByStatus.Data = new List<int>
        {
            statusGroups.FirstOrDefault(g => g.Key == AssetStatus.InUse)?.Count ?? 0,
            statusGroups.FirstOrDefault(g => g.Key == AssetStatus.InStore)?.Count ?? 0,
            statusGroups.FirstOrDefault(g => g.Key == AssetStatus.UnderMaintenance)?.Count ?? 0,
            statusGroups.FirstOrDefault(g => g.Key == AssetStatus.Discarded)?.Count ?? 0
        };
        dashboard.Charts.AssetsByStatus.Colors = new List<string> { "#0b6c78", "#19a974", "#f39c12", "#d64545" };

        // 4. Charts - Assets By Department (Division)
        var assetsByDivision = allAssets
            .GroupBy(a => a.DivisionId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToList();

        var divisions = await _context.Divisions.AsNoTracking().ToListAsync(cancellationToken);
        dashboard.Charts.AssetsByDepartment.Labels = divisions.Select(d => d.Name).ToList();
        dashboard.Charts.AssetsByDepartment.Data = divisions.Select(d => assetsByDivision.FirstOrDefault(x => x.Key == d.Id)?.Count ?? 0).ToList();
        dashboard.Charts.AssetsByDepartment.Colors = new List<string> { "#0b6c78", "#ff8c42", "#3ecf8e", "#6366f1", "#7e7f86" };

        // 5. Checkout Trend (Mock data for now since we don't have historical data)
        dashboard.Charts.CheckoutTrend.Labels = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        dashboard.Charts.CheckoutTrend.Data = new List<int> { 12, 14, 16, 13, 17, 19, 21, 19, 22, 24, 21, 24 };

        // 6. Anomalies (Mock data for now)
        dashboard.Charts.Anomalies.GhostAssets = 0;
        dashboard.Charts.Anomalies.MissingAssets = 0;
        dashboard.Charts.Anomalies.MaintenanceDue = dashboard.Kpis.MaintenanceDue;

        // 7. Recent Activity (Simplified)
        var recentAssets = await _context.Assets
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .Take(5)
            .Select(a => new RecentActivityDto
            {
                Id = a.Id.ToString(),
                Action = "registered",
                AssetName = a.AssetTag ?? a.AssetCode,
                AssetCode = a.AssetCode,
                User = a.CreatedBy ?? "System",
                Timestamp = a.CreatedAt,
                Icon = "add_circle",
                Color = "#19a974"
            })
            .ToListAsync(cancellationToken);

        dashboard.RecentActivity = recentAssets;

        return dashboard;
    }
}
