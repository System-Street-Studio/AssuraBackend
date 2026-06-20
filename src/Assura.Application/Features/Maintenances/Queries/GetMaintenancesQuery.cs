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
            .Include(m => m.Asset)
                .ThenInclude(a => a.Category)
            .Include(m => m.Asset)
                .ThenInclude(a => a.AssetRequests)
            .Include(m => m.RepairingFirm)
            .Include(m => m.RequestedByUser)
            .Include(m => m.ApprovedByUser)
            .Include(m => m.StorekeeperUser)
            .Include(m => m.ReplacementAsset)
                .ThenInclude(ra => ra!.Product)
            .AsNoTracking()
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new MaintenanceDto
            {
                Id = m.Id,
                RequesterId = m.Asset.AssetRequests
                    .Where(ar => ar.RequestType == "Maintenance" && ar.AssetId == m.AssetId)
                    .Select(ar => ar.RequesterId)
                    .FirstOrDefault(),
                MaintenanceNumber = m.MaintenanceNumber,
                Type = m.Type.ToString(),
                MaintenanceDate = m.MaintenanceDate,
                Description = m.Description,
                Cost = m.Cost,
                Status = m.Status,
                Priority = m.Priority,
                IssueType = m.IssueType,
                Notes = m.Notes,
                AssetId = m.AssetId,
                AssetName = m.Asset.Product != null ? m.Asset.Product.Name : m.Asset.AssetCode,
                AssetCode = m.Asset.AssetCode,
                CategoryId = m.Asset.CategoryId,
                CategoryName = m.Asset.Category != null ? m.Asset.Category.Name : null,
                RequestedByUserId = m.RequestedByUserId,
                RequestedByName = m.RequestedByUser != null ? m.RequestedByUser.FirstName + " " + m.RequestedByUser.LastName : null,
                RequesterDivision = m.RequestedByUser != null && m.RequestedByUser.Division != null ? m.RequestedByUser.Division.Name : null,
                ApprovedByUserId = m.ApprovedByUserId,
                ApprovedByName = m.ApprovedByUser != null ? m.ApprovedByUser.FirstName + " " + m.ApprovedByUser.LastName : null,
                StorekeeperUserId = m.StorekeeperUserId,
                StorekeeperName = m.StorekeeperUser != null ? m.StorekeeperUser.FirstName + " " + m.StorekeeperUser.LastName : null,
                ReplacementAssetId = m.ReplacementAssetId,
                ReplacementAssetCode = m.ReplacementAsset != null ? m.ReplacementAsset.AssetCode : null,
                ReplacementAssetName = m.ReplacementAsset != null && m.ReplacementAsset.Product != null ? m.ReplacementAsset.Product.Name : null,
                RepairingFirmId = m.RepairingFirmId,
                RepairingFirmName = m.RepairingFirm != null ? m.RepairingFirm.Name : null,
                OriginalRequestId = m.OriginalRequestId,
                ApprovedAt = m.ApprovedAt,
                StartedAt = m.StartedAt,
                CompletedAt = m.CompletedAt,
                EscalatedToProcurementAt = m.EscalatedToProcurementAt
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("[DEBUG] GetMaintenancesQueryHandler: Found {Count} records", maintenances.Count);
        return maintenances;
    }
}
