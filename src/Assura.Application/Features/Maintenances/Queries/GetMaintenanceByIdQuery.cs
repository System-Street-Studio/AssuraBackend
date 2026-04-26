using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Maintenances.Queries;

public record GetMaintenanceByIdQuery(int Id) : IRequest<MaintenanceDto?>;

public class GetMaintenanceByIdQueryHandler : IRequestHandler<GetMaintenanceByIdQuery, MaintenanceDto?>
{
    private readonly IApplicationDbContext _context;

    public GetMaintenanceByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MaintenanceDto?> Handle(GetMaintenanceByIdQuery request, CancellationToken cancellationToken)
    {
        var maintenance = await _context.Maintenances
            .Include(m => m.Asset)
                .ThenInclude(a => a.Product)
            .Include(m => m.RepairingFirm)
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (maintenance == null) return null;

        return new MaintenanceDto
        {
            Id = maintenance.Id,
            MaintenanceNumber = maintenance.MaintenanceNumber,
            Type = maintenance.Type.ToString(),
            MaintenanceDate = maintenance.MaintenanceDate,
            Description = maintenance.Description,
            Cost = maintenance.Cost,
            Status = maintenance.Status,
            AssetId = maintenance.AssetId,
            AssetName = maintenance.Asset?.Product?.Name ?? string.Empty,
            RepairingFirmId = maintenance.RepairingFirmId,
            RepairingFirmName = maintenance.RepairingFirm?.Name
        };
    }
}
