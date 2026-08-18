using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using Assura.Domain.Constants;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assura.Application.Features.Dashboard.Queries;

public record GetDashboardQuery(int? UserId = null) : IRequest<DashboardDto>;

/// <summary>
/// Builds the storekeeper/admin dashboard payload.
///
/// PERFORMANCE NOTE: the database lives on a remote host (~250 ms round-trip), so the cost of
/// this handler is dominated by the *number* of sequential queries, not by the amount of data
/// moved. The previous implementation issued 15 separate queries (~7.3 s end to end). This
/// version issues at most 5: every asset KPI and all three asset charts now come out of a
/// single GROUP BY, and both the lookup tables and the finished payload are memory-cached.
/// </summary>
public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    // The dashboard is a read-only overview, so a short TTL keeps it fresh enough while
    // absorbing repeat visits. Categories/Divisions are seed-style lookup data that changes
    // far less often, hence the longer TTL.
    private static readonly TimeSpan DashboardTtl = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan LookupTtl = TimeSpan.FromMinutes(10);

    private static readonly string[] ChartPalette =
        { "#0b6c78", "#ff8c42", "#3ecf8e", "#6366f1", "#7e7f86" };

    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetDashboardQueryHandler> _logger;
    private readonly IMemoryCache _cache;

    public GetDashboardQueryHandler(
        IApplicationDbContext context,
        ILogger<GetDashboardQueryHandler> logger,
        IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        // Cache per scope: a user-scoped dashboard must never be served from the unscoped
        // (all-assets) entry, and vice versa.
        var cacheKey = $"dashboard:v1:{request.UserId?.ToString() ?? "all"}";

        if (_cache.TryGetValue(cacheKey, out DashboardDto? cached) && cached is not null)
        {
            return cached;
        }

        var dashboard = await BuildDashboardAsync(request.UserId, cancellationToken);
        _cache.Set(cacheKey, dashboard, DashboardTtl);
        return dashboard;
    }

    private async Task<DashboardDto> BuildDashboardAsync(int? userId, CancellationToken cancellationToken)
    {
        var dashboard = new DashboardDto();

        try
        {
            // IgnoreAutoIncludes() avoids LEFT JOINs on Specifications that we never read here.
            var baseQuery = _context.Assets.AsNoTracking().IgnoreAutoIncludes();
            var requestsQuery = _context.Requests.AsNoTracking().Where(r => !r.IsDeleted);

            if (userId.HasValue)
            {
                baseQuery = baseQuery.Where(a => a.AssignedUserId == userId.Value);
                requestsQuery = requestsQuery.Where(r => r.RequesterId == userId.Value);
            }

            var nowUtc = DateTime.UtcNow;

            // Query 1: every asset number in a single GROUP BY.
            // Grouping on (CategoryId, DivisionId, Status) yields a small fact table that we
            // re-aggregate in memory for the KPIs *and* all three asset charts. Conditional
            // SUMs are used instead of Count(predicate) because they translate reliably to
            // SUM(CASE WHEN ... THEN 1 ELSE 0 END) on MySQL/MariaDB.
            var assetFacts = await baseQuery
                .GroupBy(a => new { a.CategoryId, a.DivisionId, a.Status })
                .Select(g => new AssetFact
                {
                    CategoryId = g.Key.CategoryId,
                    DivisionId = g.Key.DivisionId,
                    Status = g.Key.Status,
                    Count = g.Count(),
                    Value = g.Sum(a => a.PurchaseValue),
                    AwaitingPickup = g.Sum(a =>
                        a.ReservedForUserId != null &&
                        a.ReservedByRequestId != null &&
                        (a.ReservedUntilUtc == null || a.ReservedUntilUtc >= nowUtc)
                            ? 1
                            : 0),
                })
                .ToListAsync(cancellationToken);

            // Query 2: every request number in a single GROUP BY.
            var requestFacts = await requestsQuery
                .GroupBy(r => r.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            // Queries 3 and 4: lookup tables, cached for ten minutes.
            var categories = await GetLookupAsync(
                "dashboard:lookup:categories",
                ct => _context.Categories.AsNoTracking()
                    .OrderBy(c => c.Id)
                    .Select(c => new Lookup { Id = c.Id, Name = c.Name })
                    .ToListAsync(ct),
                cancellationToken);

            var divisions = await GetLookupAsync(
                "dashboard:lookup:divisions",
                ct => _context.Divisions.AsNoTracking()
                    .OrderBy(d => d.Id)
                    .Select(d => new Lookup { Id = d.Id, Name = d.Name })
                    .ToListAsync(ct),
                cancellationToken);

            // KPIs, all computed in memory from the fact table.
            dashboard.Kpis.TotalAssets = assetFacts.Sum(f => f.Count);
            dashboard.Kpis.CheckedOut = CountByStatus(assetFacts, AssetStatus.InUse);
            dashboard.Kpis.Available = CountByStatus(assetFacts, AssetStatus.InStore);
            dashboard.Kpis.MaintenanceDue = CountByStatus(assetFacts, AssetStatus.UnderMaintenance);
            dashboard.Kpis.AwaitingPickupConfirmations = assetFacts.Sum(f => f.AwaitingPickup);
            dashboard.Kpis.TotalAssetValue = $"LKR {assetFacts.Sum(f => f.Value):N0}";

            dashboard.Kpis.PendingRequests = requestFacts.Sum(r => r.Count);
            dashboard.Kpis.TemporaryAssignedAssets = requestFacts
                .Where(r => r.Status == RequestWorkflowStatus.TemporaryAssigned)
                .Sum(r => r.Count);
            dashboard.Kpis.ProcurementEscalations = requestFacts
                .Where(r => r.Status == RequestWorkflowStatus.PendingProcurement)
                .Sum(r => r.Count);

            // Charts, re-aggregated from the same fact table.
            var byCategory = assetFacts
                .GroupBy(f => f.CategoryId)
                .ToDictionary(g => g.Key ?? 0, g => g.Sum(f => f.Count));

            dashboard.Charts.AssetsByCategory.Labels = categories.Select(c => c.Name).ToList();
            dashboard.Charts.AssetsByCategory.Data = categories
                .Select(c => byCategory.TryGetValue(c.Id, out var n) ? n : 0)
                .ToList();
            dashboard.Charts.AssetsByCategory.Colors = BuildPalette(categories.Count);

            // Every AssetStatus is represented, so the bars add up to TotalAssets. Leaving
            // Transferred/Lost out made the chart silently under-report the fleet.
            var statusLabels = new List<string> { "In Use", "Available", "Maintenance", "Discarded", "Transferred", "Lost" };
            var statusData = new List<int>
            {
                dashboard.Kpis.CheckedOut,
                dashboard.Kpis.Available,
                dashboard.Kpis.MaintenanceDue,
                CountByStatus(assetFacts, AssetStatus.Discarded),
                CountByStatus(assetFacts, AssetStatus.Transferred),
                CountByStatus(assetFacts, AssetStatus.Lost),
            };
            var statusColors = new List<string> { "#0b6c78", "#19a974", "#f39c12", "#d64545", "#6366f1", "#7e7f86" };

            // Rows whose Status falls outside the enum (bad or legacy data) would otherwise
            // disappear from the chart while still counting toward TotalAssets. Surface them
            // rather than letting the bars quietly fail to reconcile.
            var unclassified = dashboard.Kpis.TotalAssets - statusData.Sum();
            if (unclassified > 0)
            {
                statusLabels.Add("Unknown");
                statusData.Add(unclassified);
                statusColors.Add("#475569");
            }

            dashboard.Charts.AssetsByStatus.Labels = statusLabels;
            dashboard.Charts.AssetsByStatus.Data = statusData;
            dashboard.Charts.AssetsByStatus.Colors = statusColors;

            var byDivision = assetFacts
                .GroupBy(f => f.DivisionId)
                .ToDictionary(g => g.Key ?? 0, g => g.Sum(f => f.Count));

            var divisionLabels = divisions.Select(d => d.Name).ToList();
            var divisionData = divisions
                .Select(d => byDivision.TryGetValue(d.Id, out var n) ? n : 0)
                .ToList();
            var divisionColors = BuildPalette(divisions.Count);

            // AssetsByDepartment is the legacy property name; both are populated so older
            // clients keep working alongside the current frontend.
            dashboard.Charts.AssetsByDepartment.Labels = divisionLabels;
            dashboard.Charts.AssetsByDepartment.Data = divisionData;
            dashboard.Charts.AssetsByDepartment.Colors = divisionColors;

            dashboard.Charts.AssetsByDivision.Labels = divisionLabels;
            dashboard.Charts.AssetsByDivision.Data = divisionData;
            dashboard.Charts.AssetsByDivision.Colors = divisionColors;

            // Checkout trend is still placeholder data: there is no historical checkout table yet.
            dashboard.Charts.CheckoutTrend.Labels = new List<string>
                { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            dashboard.Charts.CheckoutTrend.Data = new List<int>
                { 12, 14, 16, 13, 17, 19, 21, 19, 22, 24, 21, 24 };

            dashboard.Charts.Anomalies.GhostAssets = 0;
            dashboard.Charts.Anomalies.MissingAssets = 0;
            dashboard.Charts.Anomalies.MaintenanceDue = dashboard.Kpis.MaintenanceDue;

            // Query 5: recent activity.
            var recentAssetsQuery = _context.Assets
                .AsNoTracking()
                .IgnoreAutoIncludes()
                .OrderByDescending(a => a.CreatedAt)
                .AsQueryable();

            if (userId.HasValue)
            {
                recentAssetsQuery = recentAssetsQuery.Where(a => a.AssignedUserId == userId.Value);
            }

            dashboard.RecentActivity = await recentAssetsQuery
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
                    Color = "#19a974",
                })
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Dashboard] Error building dashboard data");
            // Return partial data rather than throwing.
        }

        return dashboard;
    }

    private static int CountByStatus(List<AssetFact> facts, AssetStatus status) =>
        facts.Where(f => f.Status == status).Sum(f => f.Count);

    /// <summary>Repeats the shared palette so every slice/bar gets a colour.</summary>
    private static List<string> BuildPalette(int count) =>
        Enumerable.Range(0, Math.Max(count, 0))
            .Select(i => ChartPalette[i % ChartPalette.Length])
            .ToList();

    private async Task<List<Lookup>> GetLookupAsync(
        string cacheKey,
        Func<CancellationToken, Task<List<Lookup>>> load,
        CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(cacheKey, out List<Lookup>? cached) && cached is not null)
        {
            return cached;
        }

        var loaded = await load(cancellationToken);
        _cache.Set(cacheKey, loaded, LookupTtl);
        return loaded;
    }

    /// <summary>One row of the (CategoryId, DivisionId, Status) aggregate.</summary>
    private sealed class AssetFact
    {
        public int? CategoryId { get; set; }
        public int? DivisionId { get; set; }
        public AssetStatus Status { get; set; }
        public int Count { get; set; }
        public decimal Value { get; set; }
        public int AwaitingPickup { get; set; }
    }

    private sealed class Lookup
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
