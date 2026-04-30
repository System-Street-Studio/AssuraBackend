using System.Text.Json;
using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.HR.Queries;

public record GetHrActivityLogsQuery(string? Search = null) : IRequest<List<HrActivityLogDto>>;

public class HrActivityLogDto
{
    public string Date { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Officer { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Employee { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Device { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
}

public class GetHrActivityLogsQueryHandler : IRequestHandler<GetHrActivityLogsQuery, List<HrActivityLogDto>>
{
    private readonly IApplicationDbContext _context;

    public GetHrActivityLogsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<HrActivityLogDto>> Handle(GetHrActivityLogsQuery request, CancellationToken cancellationToken)
    {
        var logs = await _context.AuditLogs
            .AsNoTracking()
            .Where(x => x.EntityName == "HR")
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var results = logs.Select(MapLog).ToList();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            results = results
                .Where(x =>
                    x.Officer.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    x.Employee.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    x.Result.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    x.Action.Contains(term, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return results;
    }

    private static HrActivityLogDto MapLog(Domain.Entities.AuditLog log)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(log.NewValues) ? "{}" : log.NewValues);
        var root = document.RootElement;

        return new HrActivityLogDto
        {
            Date = log.CreatedAt.ToString("yyyy-MM-dd"),
            Time = log.CreatedAt.ToString("hh:mm tt"),
            Officer = log.CreatedBy ?? "System",
            Action = log.Action,
            Employee = GetString(root, "employee") ?? "N/A",
            Department = GetString(root, "department") ?? "Unassigned",
            Role = GetString(root, "role") ?? "Unassigned",
            Device = BuildDevice(log.IpAddress, GetString(root, "device")),
            Notes = GetString(root, "notes") ?? string.Empty,
            Result = GetString(root, "result") ?? "Success"
        };
    }

    private static string? GetString(JsonElement root, string propertyName)
    {
        return root.TryGetProperty(propertyName, out var value) ? value.GetString() : null;
    }

    private static string BuildDevice(string? ipAddress, string? device)
    {
        var ip = string.IsNullOrWhiteSpace(ipAddress) ? "Unknown IP" : ipAddress;
        var browser = string.IsNullOrWhiteSpace(device) ? "Unknown Device" : device;
        return $"{ip} {browser}".Trim();
    }
}
