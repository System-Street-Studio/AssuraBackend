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
        return await _context.Maintenances
            .Include(m => m.Asset).ThenInclude(a => a.Product)
            .Include(m => m.Asset).ThenInclude(a => a.Category)
            .Include(m => m.RepairingFirm)
            .Include(m => m.RequestedByUser)
            .Include(m => m.ApprovedByUser)
            .Include(m => m.StorekeeperUser)
            .Include(m => m.ReplacementAsset).ThenInclude(ra => ra!.Product)
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
            .FirstOrDefaultAsync(cancellationToken);
    }
}
