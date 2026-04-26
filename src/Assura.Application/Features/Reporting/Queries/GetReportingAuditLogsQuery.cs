using Assura.Application.Common.Interfaces;
using Assura.Application.Features.Reporting.DTOs;
using Assura.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Reporting.Queries;

public record GetReportingAuditLogsQuery : IRequest<ReportingAuditLogPageDto>;

public class GetReportingAuditLogsQueryHandler : IRequestHandler<GetReportingAuditLogsQuery, ReportingAuditLogPageDto>
{
    private readonly IApplicationDbContext _context;

    public GetReportingAuditLogsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReportingAuditLogPageDto> Handle(GetReportingAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var logs = await _context.AuditLogs
            .AsNoTracking()
            .Where(l => !l.IsDeleted)
            .OrderByDescending(l => l.CreatedAt)
            .Take(100)
            .ToListAsync(cancellationToken);

        var userLookup = await BuildUserLookupAsync(logs, cancellationToken);

        var logDtos = logs.Select(log =>
        {
            userLookup.TryGetValue(log.CreatedBy ?? string.Empty, out var actor);
            var status = ReportingQueryHelpers.ClassifyLogStatus(log);

            return new ReportingAuditLogEntryDto
            {
                Time = log.CreatedAt.ToLocalTime().ToString("hh:mm tt"),
                Date = log.CreatedAt.ToLocalTime().ToString("MMM dd, yyyy"),
                Actor = ReportingQueryHelpers.ResolveActorDisplay(actor, log.CreatedBy),
                Role = ReportingQueryHelpers.ResolveRoleDisplay(actor),
                Action = $"{log.Action} {log.EntityName}".Trim(),
                Detail = ReportingQueryHelpers.BuildLogDetail(log),
                Asset = string.IsNullOrWhiteSpace(log.EntityId) ? "-" : log.EntityId,
                Module = ReportingQueryHelpers.ResolveModule(log.EntityName),
                Ip = string.IsNullOrWhiteSpace(log.IpAddress) ? "-" : log.IpAddress,
                Status = status
            };
        }).ToList();

        return new ReportingAuditLogPageDto
        {
            Stats =
            [
                new ReportingStatCardDto
                {
                    Label = "Successful Events",
                    Value = logDtos.Count(l => l.Status is "Completed" or "Active").ToString("N0"),
                    Tone = "success"
                },
                new ReportingStatCardDto
                {
                    Label = "Flagged Reviews",
                    Value = logDtos.Count(l => l.Status == "Flagged").ToString("N0"),
                    Tone = "warning"
                },
                new ReportingStatCardDto
                {
                    Label = "Failed Exports",
                    Value = logDtos.Count(l => l.Status == "Failed" && l.Module == "Exports").ToString("N0"),
                    Tone = "danger"
                },
                new ReportingStatCardDto
                {
                    Label = "Active Monitors",
                    Value = logDtos.Select(l => l.Module).Distinct(StringComparer.OrdinalIgnoreCase).Count().ToString("N0"),
                    Tone = "neutral"
                }
            ],
            Logs = logDtos
        };
    }

    private async Task<Dictionary<string, User>> BuildUserLookupAsync(IEnumerable<AuditLog> logs, CancellationToken cancellationToken)
    {
        var rawValues = logs
            .Select(l => l.CreatedBy)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var numericIds = rawValues
            .Where(value => int.TryParse(value, out _))
            .Select(int.Parse)
            .ToList();

        var usernames = rawValues
            .Where(value => !int.TryParse(value, out _))
            .ToList();

        var users = await _context.Users
            .AsNoTracking()
            .Where(u => numericIds.Contains(u.Id) || usernames.Contains(u.Username))
            .ToListAsync(cancellationToken);

        var lookup = new Dictionary<string, User>(StringComparer.OrdinalIgnoreCase);

        foreach (var user in users)
        {
            lookup[user.Id.ToString()] = user;
            lookup[user.Username] = user;
        }

        return lookup;
    }
}
