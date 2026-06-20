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

        return new MaintenanceStatsDto
        {
            Total = all.Count,
            PendingApproval = all.Count(m => m.Status == "PendingApproval"),
            Approved = all.Count(m => m.Status == "Approved"),
            InProgress = all.Count(m => m.Status == "InProgress"),
            TempAssigned = all.Count(m => m.Status == "TempAssigned"),
            SentForRepair = all.Count(m => m.Status == "SentForRepair"),
            EscalatedToProcurement = all.Count(m => m.Status == "EscalatedToProcurement"),
            Completed = all.Count(m => m.Status == "Completed"),
            Rejected = all.Count(m => m.Status == "Rejected"),
        };
    }
}
