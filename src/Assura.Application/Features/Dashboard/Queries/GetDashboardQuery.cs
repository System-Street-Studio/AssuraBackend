using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using Assura.Domain.Constants;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assura.Application.Features.Dashboard.Queries;

public record GetDashboardQuery(int? UserId = null) : IRequest<DashboardDto>;

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

        // Build base query - use IgnoreAutoIncludes() to avoid LEFT JOINs on Specifications
        var baseQuery = _context.Assets.AsNoTracking().IgnoreAutoIncludes();
        var requestsQuery = _context.Requests.AsNoTracking().Where(r => !r.IsDeleted);

        if (request.UserId.HasValue)
        {
            baseQuery = baseQuery.Where(a => a.AssignedUserId == request.UserId.Value);
            requestsQuery = requestsQuery.Where(r => r.RequesterId == request.UserId.Value);
        }

        // 1. KPIs - Use database-level aggregation (no entity materialization)
        dashboard.Kpis.TotalAssets = await baseQuery.CountAsync(cancellationToken);
        dashboard.Kpis.CheckedOut = await baseQuery.CountAsync(a => a.Status == AssetStatus.InUse, cancellationToken);
        dashboard.Kpis.Available = await baseQuery.CountAsync(a => a.Status == AssetStatus.InStore, cancellationToken);
        dashboard.Kpis.MaintenanceDue = await baseQuery.CountAsync(a => a.Status == AssetStatus.UnderMaintenance, cancellationToken);

        var totalValue = await baseQuery.SumAsync(a => a.PurchaseValue, cancellationToken);
        dashboard.Kpis.TotalAssetValue = $"LKR {totalValue:N0}";

        dashboard.Kpis.PendingRequests = await requestsQuery.CountAsync(cancellationToken);
        dashboard.Kpis.TemporaryAssignedAssets = await requestsQuery
            .CountAsync(r => r.Status == RequestWorkflowStatus.TemporaryAssigned, cancellationToken);

        var nowUtc = DateTime.UtcNow;
        dashboard.Kpis.AwaitingPickupConfirmations = await baseQuery.CountAsync(a =>
            a.ReservedForUserId.HasValue &&
            a.ReservedByRequestId.HasValue &&
            (!a.ReservedUntilUtc.HasValue || a.ReservedUntilUtc.Value >= nowUtc), cancellationToken);

        dashboard.Kpis.ProcurementEscalations = await requestsQuery
            .CountAsync(r => r.Status == RequestWorkflowStatus.PendingProcurement, cancellationToken);

        // 2. Charts - Assets By Category (database-level GroupBy)
        var assetsByCategory = await baseQuery
            .GroupBy(a => a.CategoryId)
            .Select(g => new { Key = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var categories = await _context.Categories.AsNoTracking().ToListAsync(cancellationToken);
        dashboard.Charts.AssetsByCategory.Labels = categories.Select(c => c.Name).ToList();
        dashboard.Charts.AssetsByCategory.Data = categories.Select(c => assetsByCategory.FirstOrDefault(x => x.Key == c.Id)?.Count ?? 0).ToList();
        dashboard.Charts.AssetsByCategory.Colors = new List<string> { "#0b6c78", "#ff8c42", "#3ecf8e", "#6366f1", "#7e7f86" };

        // 3. Charts - Assets By Status (database-level GroupBy)
        var statusGroups = await baseQuery
            .GroupBy(a => a.Status)
            .Select(g => new { Key = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
        dashboard.Charts.AssetsByStatus.Labels = new List<string> { "In Use", "Available", "Maintenance", "Discarded" };
        dashboard.Charts.AssetsByStatus.Data = new List<int>
        {
            statusGroups.FirstOrDefault(g => g.Key == AssetStatus.InUse)?.Count ?? 0,
            statusGroups.FirstOrDefault(g => g.Key == AssetStatus.InStore)?.Count ?? 0,
            statusGroups.FirstOrDefault(g => g.Key == AssetStatus.UnderMaintenance)?.Count ?? 0,
            statusGroups.FirstOrDefault(g => g.Key == AssetStatus.Discarded)?.Count ?? 0
        };
        dashboard.Charts.AssetsByStatus.Colors = new List<string> { "#0b6c78", "#19a974", "#f39c12", "#d64545" };

        // 4. Charts - Assets By Department (Division) (database-level GroupBy)
        var assetsByDivision = await baseQuery
            .GroupBy(a => a.DivisionId)
            .Select(g => new { Key = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

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

        // 7. Recent Activity - Use .Select() projection to avoid entity materialization
        var recentAssetsQuery = _context.Assets
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .OrderByDescending(a => a.CreatedAt)
            .AsQueryable();

        if (request.UserId.HasValue)
        {
            recentAssetsQuery = recentAssetsQuery.Where(a => a.AssignedUserId == request.UserId.Value);
        }

        var recentAssets = await recentAssetsQuery
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
