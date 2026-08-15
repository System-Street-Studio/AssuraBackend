using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Maintenances.Queries;

public record GetMaintenanceStatsQuery : IRequest<MaintenanceStatsDto>;

public class GetMaintenanceStatsQueryHandler : IRequestHandler<GetMaintenanceStatsQuery, MaintenanceStatsDto>
{
    private readonly IApplicationDbContext _context;

    public GetMaintenanceStatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MaintenanceStatsDto> Handle(GetMaintenanceStatsQuery request, CancellationToken cancellationToken)
    {
        var all = await _context.Maintenances.AsNoTracking().ToListAsync(cancellationToken);

        // Status values are free-text (Maintenance.Status has no enum backing it), and
        // different write paths/seed data have stored "In Progress" alongside the
        // canonical "InProgress" the frontend filters on, plus the legacy short form
        // "Pending" for what is now "PendingApproval". Normalize so every record lands
        // in the bucket it actually belongs to instead of being silently dropped.
        static string Normalize(string? status) => (status ?? string.Empty).Replace(" ", "");

        return new MaintenanceStatsDto
        {
            Total = all.Count,
            PendingApproval = all.Count(m => Normalize(m.Status) is "PendingApproval" or "Pending"),
            Approved = all.Count(m => Normalize(m.Status) == "Approved"),
            InProgress = all.Count(m => Normalize(m.Status) == "InProgress"),
            TempAssigned = all.Count(m => Normalize(m.Status) == "TempAssigned"),
            SentForRepair = all.Count(m => Normalize(m.Status) == "SentForRepair"),
            EscalatedToProcurement = all.Count(m => Normalize(m.Status) == "EscalatedToProcurement"),
            Completed = all.Count(m => Normalize(m.Status) == "Completed"),
            Rejected = all.Count(m => Normalize(m.Status) == "Rejected"),
        };
    }
}
