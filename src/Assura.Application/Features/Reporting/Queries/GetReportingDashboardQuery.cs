using Assura.Application.Common.Interfaces;
using Assura.Application.Features.Reporting.DTOs;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Reporting.Queries;

public record GetReportingDashboardQuery : IRequest<ReportingDashboardDto>;

public class GetReportingDashboardQueryHandler : IRequestHandler<GetReportingDashboardQuery, ReportingDashboardDto>
{
    private readonly IApplicationDbContext _context;

    public GetReportingDashboardQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReportingDashboardDto> Handle(GetReportingDashboardQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var assets = await _context.Assets
            .AsNoTracking()
            .Where(a => !a.IsDeleted)
            .Select(a => new
            {
                a.Id,
                a.AssetCode,
                a.AssetTag,
                a.SerialNumber,
                a.AssetDate,
                a.PurchaseValue,
                Status = (AssetStatus?)a.Status,
                CategoryName = a.Category != null ? a.Category.Name : "Unknown",
                DivisionName = a.Division != null ? a.Division.Name : "Unknown"
            })
            .ToListAsync(cancellationToken);

        var auditLogs = await _context.AuditLogs
            .AsNoTracking()
            .Where(l => !l.IsDeleted)
            .ToListAsync(cancellationToken);

        var totalAssets = assets.Count;
        var divisionsRepresented = assets
            .Where(a => !string.IsNullOrWhiteSpace(a.DivisionName))
            .Select(a => a.DivisionName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        var ghostAssets = assets.Count(a =>
            string.IsNullOrWhiteSpace(a.AssetTag) &&
            string.IsNullOrWhiteSpace(a.SerialNumber));

        var missingVerification = assets.Count(a =>
            a.Status.HasValue && (a.Status == AssetStatus.Lost ||
            a.Status == AssetStatus.Transferred));

        var categoryGroups = assets
            .GroupBy(a => a.CategoryName)
            .OrderByDescending(g => g.Count())
            .ToList();

        var divisionGroups = assets
            .GroupBy(a => a.DivisionName)
            .OrderByDescending(g => g.Count())
            .Take(6)
            .ToList();

        var lostItemsCount = await _context.LostItems
            .AsNoTracking()
            .Where(l => !l.IsDeleted && (l.Status == LostItemStatus.ConfirmedLost || l.Status == LostItemStatus.Reported))
            .CountAsync(cancellationToken);

        var inStoreCount = assets.Count(a => a.Status == AssetStatus.InStore);
        var activeCount = assets.Count(a => a.Status == AssetStatus.InUse);
        var maintenanceCount = assets.Count(a => a.Status == AssetStatus.UnderMaintenance);
        var discardedCount = assets.Count(a => a.Status == AssetStatus.Discarded);
        var transferredCount = assets.Count(a => a.Status == AssetStatus.Transferred);
        var lostCount = Math.Max(assets.Count(a => a.Status == AssetStatus.Lost), lostItemsCount);

        var totalFleet = totalAssets > 0 ? totalAssets : (inStoreCount + activeCount + maintenanceCount + discardedCount + transferredCount + lostCount);

        var statusItems = new List<(string Label, int Count, string Color)>
        {
            ("In Store", inStoreCount, ReportingQueryHelpers.GetColor(0)),
            ("Active", activeCount, ReportingQueryHelpers.GetColor(1)),
            ("Discarded", discardedCount, ReportingQueryHelpers.GetColor(2)),
            ("Maintenance", maintenanceCount, ReportingQueryHelpers.GetColor(3)),
            ("Transferred", transferredCount, ReportingQueryHelpers.GetColor(4)),
            ("Lost", lostCount, ReportingQueryHelpers.GetColor(5))
        };

        var valueGroups = assets
            .GroupBy(a => a.CategoryName)
            .Select(g => new { Category = g.Key, TotalValue = g.Sum(x => x.PurchaseValue) })
            .OrderByDescending(g => g.TotalValue)
            .Take(6)
            .ToList();

        var totalAssetValue = valueGroups.Sum(g => g.TotalValue);

        return new ReportingDashboardDto
        {
            Metrics =
            [
                new ReportingMetricDto { Label = "Total Assets", Value = totalAssets.ToString("N0"), Accent = totalAssets > 0 },
                new ReportingMetricDto { Label = "Audited This Month", Value = auditLogs.Count(l => l.CreatedAt >= monthStart).ToString("N0") },
                new ReportingMetricDto { Label = "Flagged Exceptions", Value = auditLogs.Count(l => ReportingQueryHelpers.ClassifyLogStatus(l) == "Flagged").ToString("N0"), Accent = true },
                new ReportingMetricDto { Label = "Verified Locations", Value = divisionsRepresented.ToString("N0") }
            ],
            CategoryLegend = categoryGroups
                .Select((group, index) => new ReportingLegendItemDto
                {
                    Label = group.Key,
                    Count = group.Count(),
                    Percentage = ReportingQueryHelpers.ToPercent(group.Count(), totalAssets),
                    Color = ReportingQueryHelpers.GetColor(index)
                })
                .ToList(),
            StatusBars = statusItems
                .Select(item => new ReportingBarItemDto
                {
                    Label = item.Label,
                    RawValue = item.Count,
                    Value = ReportingQueryHelpers.ToPercent(item.Count, totalFleet),
                    Color = item.Color
                })
                .ToList(),
            DivisionBars = divisionGroups
                .Select((group, index) => new ReportingBarItemDto
                {
                    Label = group.Key,
                    RawValue = group.Count(),
                    Value = ReportingQueryHelpers.ToPercent(group.Count(), totalAssets),
                    Color = ReportingQueryHelpers.GetColor(index)
                })
                .ToList(),
            ValueBars = valueGroups
                .Select((group, index) => new ReportingBarItemDto
                {
                    Label = group.Category,
                    RawValue = group.TotalValue,
                    Value = ReportingQueryHelpers.ToPercent(group.TotalValue, totalAssetValue),
                    Color = ReportingQueryHelpers.GetColor(index)
                })
                .ToList(),
            Anomalies = new ReportingAnomaliesDto
            {
                GhostAssetsDetected = ghostAssets,
                MissingPhysicalVerification = missingVerification
            }
        };
    }
}
