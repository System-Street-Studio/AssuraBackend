using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Assura.Application.Features.Maintenances.Queries;

public record GetMaintenancesQuery : IRequest<List<MaintenanceDto>>;

public class GetMaintenancesQueryHandler : IRequestHandler<GetMaintenancesQuery, List<MaintenanceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetMaintenancesQueryHandler> _logger;

    public GetMaintenancesQueryHandler(IApplicationDbContext context, ILogger<GetMaintenancesQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<MaintenanceDto>> Handle(GetMaintenancesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[DEBUG] GetMaintenancesQueryHandler: Fetching maintenance records from DB");
        
        var maintenances = await _context.Maintenances
            .Include(m => m.Asset)
                .ThenInclude(a => a.Product)
            .Include(m => m.RepairingFirm)
            .AsNoTracking()
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
            .ToListAsync(cancellationToken);

        _logger.LogInformation("[DEBUG] GetMaintenancesQueryHandler: Found {Count} records", maintenances.Count);
        return maintenances;
    }
}
