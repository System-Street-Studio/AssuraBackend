using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using Assura.Domain.Constants;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<GetDashboardQueryHandler> _logger;

    public GetDashboardQueryHandler(IApplicationDbContext context, ILogger<GetDashboardQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var dashboard = new DashboardDto();

        try
        {
            // 1. KPIs
            var allAssetsQuery = _context.Assets.AsNoTracking();
            var requestsQuery = _context.Requests.AsNoTracking().Where(r => !r.IsDeleted);

            if (request.UserId.HasValue)
            {
                allAssetsQuery = allAssetsQuery.Where(a => a.AssignedUserId == request.UserId.Value);
                requestsQuery = requestsQuery.Where(r => r.RequesterId == request.UserId.Value);
            }

            var allAssets = await allAssetsQuery
                .Select(a => new 
                { 
                    a.Status, 
                    a.PurchaseValue, 
                    a.CategoryId, 
                    a.DivisionId,
                    a.ReservedForUserId,
                    a.ReservedByRequestId,
                    a.ReservedUntilUtc
                })
                .ToListAsync(cancellationToken);
            dashboard.Kpis.TotalAssets = allAssets.Count;
            dashboard.Kpis.CheckedOut = allAssets.Count(a => a.Status == AssetStatus.InUse);
            dashboard.Kpis.Available = allAssets.Count(a => a.Status == AssetStatus.InStore);
            dashboard.Kpis.MaintenanceDue = allAssets.Count(a => a.Status == AssetStatus.UnderMaintenance);

            var totalValue = allAssets.Sum(a => a.PurchaseValue);
            dashboard.Kpis.TotalAssetValue = $"LKR {totalValue:N0}";

            dashboard.Kpis.PendingRequests = await requestsQuery.CountAsync(cancellationToken);
            dashboard.Kpis.TemporaryAssignedAssets = await requestsQuery
                .CountAsync(r => r.Status == RequestWorkflowStatus.TemporaryAssigned, cancellationToken);

            var nowUtc = DateTime.UtcNow;
            dashboard.Kpis.AwaitingPickupConfirmations = allAssets.Count(a =>
                a.ReservedForUserId.HasValue &&
                a.ReservedByRequestId.HasValue &&
                (!a.ReservedUntilUtc.HasValue || a.ReservedUntilUtc.Value >= nowUtc));

            dashboard.Kpis.ProcurementEscalations = await requestsQuery
                .CountAsync(r => r.Status == RequestWorkflowStatus.PendingProcurement, cancellationToken);

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

            // 4. Charts - Assets By Division
            var assetsByDivision = allAssets
                .GroupBy(a => a.DivisionId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToList();

            var divisions = await _context.Divisions.AsNoTracking().ToListAsync(cancellationToken);
            var divisionLabels = divisions.Select(d => d.Name).ToList();
            var divisionData = divisions.Select(d => assetsByDivision.FirstOrDefault(x => x.Key == d.Id)?.Count ?? 0).ToList();
            var divisionColors = new List<string> { "#0b6c78", "#ff8c42", "#3ecf8e", "#6366f1", "#7e7f86" };

            // Populate both properties for frontend compatibility
            dashboard.Charts.AssetsByDepartment.Labels = divisionLabels;
            dashboard.Charts.AssetsByDepartment.Data = divisionData;
            dashboard.Charts.AssetsByDepartment.Colors = divisionColors;

            dashboard.Charts.AssetsByDivision.Labels = divisionLabels;
            dashboard.Charts.AssetsByDivision.Data = divisionData;
            dashboard.Charts.AssetsByDivision.Colors = divisionColors;

            // 5. Checkout Trend (Mock data for now since we don't have historical data)
            dashboard.Charts.CheckoutTrend.Labels = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            dashboard.Charts.CheckoutTrend.Data = new List<int> { 12, 14, 16, 13, 17, 19, 21, 19, 22, 24, 21, 24 };

            // 6. Anomalies
            dashboard.Charts.Anomalies.GhostAssets = 0;
            dashboard.Charts.Anomalies.MissingAssets = 0;
            dashboard.Charts.Anomalies.MaintenanceDue = dashboard.Kpis.MaintenanceDue;

            // 7. Recent Activity
            var recentAssetsQuery = _context.Assets
                .AsNoTracking()
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Dashboard] Error building dashboard data");
            // Return partial data rather than throwing
        }

        return dashboard;
    }
}
