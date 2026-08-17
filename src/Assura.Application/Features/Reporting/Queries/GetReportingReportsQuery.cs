using Assura.Application.Common.Interfaces;
using Assura.Application.Features.Reporting.DTOs;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Reporting.Queries;

public record GetReportingReportsQuery : IRequest<ReportingReportsPageDto>;

public class GetReportingReportsQueryHandler : IRequestHandler<GetReportingReportsQuery, ReportingReportsPageDto>
{
    private readonly IApplicationDbContext _context;

    public GetReportingReportsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReportingReportsPageDto> Handle(GetReportingReportsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var quarterStartMonth = ((now.Month - 1) / 3) * 3 + 1;
        var quarterStart = new DateTime(now.Year, quarterStartMonth, 1);

        var assets = await _context.Assets
            .AsNoTracking()
            .Where(a => !a.IsDeleted)
            .Select(a => new
            {
                a.PurchaseValue,
                Status = (AssetStatus?)a.Status,
                a.DivisionId,
            })
            .ToListAsync(cancellationToken);

        var auditLogs = await _context.AuditLogs
            .AsNoTracking()
            .Where(l => !l.IsDeleted)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);

        var totalValue = assets.Sum(a => a.PurchaseValue);
        var discardedAssets = assets.Count(a => a.Status == AssetStatus.Discarded);
        var flaggedLogs = auditLogs.Count(l => ReportingQueryHelpers.ClassifyLogStatus(l) == "Flagged");
        var completedLogs = auditLogs.Count(l => ReportingQueryHelpers.ClassifyLogStatus(l) == "Completed");

        var customReports = await _context.CustomReports
            .AsNoTracking()
            .Where(r => !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReportingReportItemDto
            {
                Id = r.ReportIdCode,
                Title = r.Title,
                Owner = r.Owner,
                Type = r.Type,
                Period = r.Period,
                Generated = r.CreatedAt.ToLocalTime().ToString("MMM dd, yyyy"),
                Status = r.Status,
                Size = r.Size,
                IsSystemGenerated = false
            })
            .ToListAsync(cancellationToken);

        var reportItems = new List<ReportingReportItemDto>();
        reportItems.AddRange(customReports);
        reportItems.AddRange(new List<ReportingReportItemDto>
        {
            new()
            {
                Id = $"RPT-{now:yyyyMM}-001",
                Title = "Monthly Asset Verification",
                Owner = "System",
                Type = "Audit",
                Period = monthStart.ToString("MMM yyyy"),
                Generated = now.ToLocalTime().ToString("MMM dd, yyyy"),
                Status = completedLogs > 0 ? "Completed" : "Pending",
                Size = $"{Math.Max(1, assets.Count / 25)}.0 MB",
                IsSystemGenerated = true
            },
            new()
            {
                Id = $"RPT-{now:yyyyMM}-002",
                Title = "Division Variance Register",
                Owner = "System",
                Type = "Exception",
                Period = $"Q{((now.Month - 1) / 3) + 1} {now.Year}",
                Generated = now.ToLocalTime().ToString("MMM dd, yyyy"),
                Status = flaggedLogs > 0 ? "Pending" : "Completed",
                Size = $"{Math.Max(1, flaggedLogs)} KB",
                IsSystemGenerated = true
            },
            new()
            {
                Id = $"RPT-{now:yyyyMM}-003",
                Title = "Lifecycle Disposal Summary",
                Owner = "System",
                Type = "Lifecycle",
                Period = now.Year.ToString(),
                Generated = now.ToLocalTime().ToString("MMM dd, yyyy"),
                Status = discardedAssets > 0 ? "Completed" : "Pending",
                Size = $"{Math.Max(1, discardedAssets)} KB",
                IsSystemGenerated = true
            },
            new()
            {
                Id = $"RPT-{now:yyyyMM}-004",
                Title = "Asset Value Snapshot",
                Owner = "System",
                Type = "Finance",
                Period = quarterStart.ToString("MMM yyyy"),
                Generated = now.ToLocalTime().ToString("MMM dd, yyyy"),
                Status = totalValue > 0 ? "Completed" : "Pending",
                Size = $"{Math.Max(1, assets.Select(a => a.DivisionId).Distinct().Count())}.2 MB",
                IsSystemGenerated = true
            }
        });

        var insights = new List<ReportingInsightDto>
        {
            new()
            {
                Title = "High variance in flagged activity",
                Detail = flaggedLogs == 0
                    ? "No flagged audit activity is currently recorded."
                    : $"{flaggedLogs} audit events are currently marked as flagged and may need review.",
                Tone = flaggedLogs > 0 ? "warning" : "success"
            },
            new()
            {
                Title = "Lifecycle disposal pressure",
                Detail = discardedAssets == 0
                    ? "No discarded assets are recorded in the current dataset."
                    : $"{discardedAssets} assets are already marked discarded and should appear in disposal summaries.",
                Tone = discardedAssets > 0 ? "danger" : "success"
            },
            new()
            {
                Title = "Quarterly value coverage",
                Detail = $"Current tracked asset value is {totalValue:N0}, aggregated from {assets.Count} assets.",
                Tone = "success"
            }
        };

        return new ReportingReportsPageDto
        {
            Summaries =
            [
                new ReportingStatCardDto
                {
                    Label = "Generated Today",
                    Value = auditLogs.Count(l => l.CreatedAt >= DateTime.UtcNow.Date).ToString("N0"),
                    Tone = "success"
                },
                new ReportingStatCardDto
                {
                    Label = "Scheduled Reports",
                    Value = reportItems.Count(r => r.Status == "Pending").ToString("N0"),
                    Tone = "neutral"
                },
                new ReportingStatCardDto
                {
                    Label = "Needs Review",
                    Value = flaggedLogs.ToString("N0"),
                    Tone = "warning"
                },
                new ReportingStatCardDto
                {
                    Label = "Export Failures",
                    Value = auditLogs.Count(l =>
                        ReportingQueryHelpers.ClassifyLogStatus(l) == "Failed" &&
                        ReportingQueryHelpers.ResolveModule(l.EntityName) == "Exports").ToString("N0"),
                    Tone = "danger"
                }
            ],
            ReportItems = reportItems,
            Insights = insights
        };
    }
}
