using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.PurchasingOrders.Queries;

public record GetProcurementStatsQuery : IRequest<ProcurementStatsDto>;

public class GetProcurementStatsQueryHandler : IRequestHandler<GetProcurementStatsQuery, ProcurementStatsDto>
{
    private readonly IApplicationDbContext _context;

    public GetProcurementStatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProcurementStatsDto> Handle(GetProcurementStatsQuery request, CancellationToken cancellationToken)
    {
        var totalSuppliers = await _context.Suppliers.CountAsync(cancellationToken);

        var pos = await _context.PurchasingOrders
            .Select(p => new { p.Status })
            .ToListAsync(cancellationToken);

        var posCompleted = pos.Count(p => p.Status == "Completed");
        var posNotCompleted = pos.Count - posCompleted;

        var repairs = await _context.Maintenances
            .Select(m => new { m.Status })
            .ToListAsync(cancellationToken);

        var repairsCompleted = repairs.Count(r => r.Status == "Completed");
        var repairsNotCompleted = repairs.Count - repairsCompleted;

        return new ProcurementStatsDto
        {
            TotalSuppliers = totalSuppliers,
            PosCompleted = posCompleted,
            PosNotCompleted = posNotCompleted,
            RepairsCompleted = repairsCompleted,
            RepairsNotCompleted = repairsNotCompleted
        };
    }
}
