using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Assura.Application.Features.Maintenances.Commands;

public record AssignTemporaryAssetCommand : IRequest
{
    public int MaintenanceId { get; init; }
    public int ReplacementAssetId { get; init; }
    public int StorekeeperUserId { get; init; }
    public string? Notes { get; init; }
    public bool IsDivisionHead { get; init; }
}

public class AssignTemporaryAssetCommandHandler : IRequestHandler<AssignTemporaryAssetCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AssignTemporaryAssetCommandHandler> _logger;

    public AssignTemporaryAssetCommandHandler(IApplicationDbContext context, ILogger<AssignTemporaryAssetCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(AssignTemporaryAssetCommand request, CancellationToken cancellationToken)
    {
        var maintenance = await _context.Maintenances
            .Include(m => m.Asset)
            .FirstOrDefaultAsync(m => m.Id == request.MaintenanceId, cancellationToken)
            ?? throw new Exception($"Maintenance {request.MaintenanceId} not found");

        // Division Heads may only act on maintenance records for assets in their own
        // division; Admin/Procurement/Storekeeper/Maintenance roles remain fully
        // privileged, matching the scoping pattern already used for asset requests
        // (ApproveAssetRequestCommand).
        if (request.IsDivisionHead)
        {
            var caller = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.StorekeeperUserId, cancellationToken);
            if (caller?.DivisionId == null || maintenance.Asset.DivisionId == null || caller.DivisionId != maintenance.Asset.DivisionId)
            {
                throw new UnauthorizedAccessException("Division Head may only act on maintenance records within their own division.");
            }
        }

        var replacementAsset = await _context.Assets
            .FirstOrDefaultAsync(a => a.Id == request.ReplacementAssetId, cancellationToken)
            ?? throw new Exception($"Replacement asset {request.ReplacementAssetId} not found");

        if (replacementAsset.Status != AssetStatus.InStore)
            throw new Exception("Replacement asset is not available in store");

        // Mark the original asset as Under Maintenance
        maintenance.Asset.Status = AssetStatus.UnderMaintenance;

        // Assign replacement to the same user as the original asset
        replacementAsset.Status = AssetStatus.InUse;
        if (maintenance.RequestedByUserId.HasValue)
        {
            replacementAsset.AssignedUserId = maintenance.RequestedByUserId;
        }

        // Update maintenance record
        maintenance.ReplacementAssetId = request.ReplacementAssetId;
        maintenance.StorekeeperUserId = request.StorekeeperUserId;
        maintenance.Status = "TempAssigned";
        maintenance.StartedAt ??= DateTime.UtcNow;
        if (!string.IsNullOrEmpty(request.Notes))
            maintenance.Notes = request.Notes;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("[Maintenance] Temp asset {ReplacementId} assigned for maintenance {Id} by storekeeper {UserId}",
            request.ReplacementAssetId, request.MaintenanceId, request.StorekeeperUserId);
    }
}
