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
        var dto = await _context.Maintenances
            .Include(m => m.Asset)
                .ThenInclude(a => a.Product)
            .Include(m => m.RepairingFirm)
            .AsNoTracking()
            .Where(m => m.Id == request.Id)
            .Select(m => new MaintenanceDto
            {
                Id = m.Id,
                MaintenanceNumber = m.MaintenanceNumber,
                Type = m.Type.ToString(),
                MaintenanceDate = m.MaintenanceDate,
                Description = m.Description,
                Cost = m.Cost,
                Status = m.Status,
                AssetId = m.AssetId,
                AssetName = m.Asset.Product.Name,
                RepairingFirmId = m.RepairingFirmId,
                RepairingFirmName = m.RepairingFirm != null ? m.RepairingFirm.Name : null
            })
            .FirstOrDefaultAsync(cancellationToken);

        return dto;
    }
}
