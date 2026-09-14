using Assura.Application.Common.Interfaces;
using Assura.Domain.Constants;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Assura.Application.Features.Maintenances.Commands;

public record UpdateMaintenanceStatusCommand(int MaintenanceId, string NewStatus, int UserId, bool IsDivisionHead = false) : IRequest;

public class UpdateMaintenanceStatusCommandHandler : IRequestHandler<UpdateMaintenanceStatusCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateMaintenanceStatusCommandHandler> _logger;

    public UpdateMaintenanceStatusCommandHandler(IApplicationDbContext context, ILogger<UpdateMaintenanceStatusCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(UpdateMaintenanceStatusCommand request, CancellationToken cancellationToken)
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
            var caller = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            if (caller?.DivisionId == null || maintenance.Asset.DivisionId == null || caller.DivisionId != maintenance.Asset.DivisionId)
            {
                throw new UnauthorizedAccessException("Division Head may only act on maintenance records within their own division.");
            }
        }

        maintenance.Status = request.NewStatus;

        switch (request.NewStatus)
        {
            case "Approved":
                maintenance.ApprovedByUserId = request.UserId;
                maintenance.ApprovedAt = DateTime.UtcNow;
                break;
            case "InProgress":
                maintenance.StorekeeperUserId = request.UserId;
                maintenance.StartedAt = DateTime.UtcNow;
                break;
            case "Completed":
                maintenance.CompletedAt = DateTime.UtcNow;

                // Make asset ready upon maintenance completion
                if (maintenance.Asset != null)
                {
                    if (maintenance.RequestedByUserId.HasValue && maintenance.RequestedByUserId.Value > 0)
                    {
                        maintenance.Asset.Status = AssetStatus.InUse;
                        maintenance.Asset.AssignedUserId = maintenance.RequestedByUserId.Value;
                    }
                    else if (maintenance.Asset.AssignedUserId.HasValue && maintenance.Asset.AssignedUserId.Value > 0)
                    {
                        maintenance.Asset.Status = AssetStatus.InUse;
                    }
                    else
                    {
                        maintenance.Asset.Status = AssetStatus.InStore;
                        maintenance.Asset.AssignedUserId = null;
                    }
                    maintenance.Asset.UpdatedAt = DateTime.UtcNow;
                }

                // Return replacement asset to store
                if (maintenance.ReplacementAssetId.HasValue)
                {
                    var rep = await _context.Assets.FirstOrDefaultAsync(a => a.Id == maintenance.ReplacementAssetId.Value, cancellationToken);
                    if (rep != null)
                    {
                        rep.Status = AssetStatus.InStore;
                        rep.AssignedUserId = null;
                        rep.UpdatedAt = DateTime.UtcNow;
                    }
                }

                // Resolve original request if linked
                if (maintenance.OriginalRequestId.HasValue)
                {
                    var origAssetReq = await _context.AssetRequests.FirstOrDefaultAsync(ar => ar.Id == maintenance.OriginalRequestId.Value, cancellationToken);
                    if (origAssetReq != null) origAssetReq.Status = RequestStatus.Completed;

                    var origReq = await _context.Requests.FirstOrDefaultAsync(r => r.Id == maintenance.OriginalRequestId.Value, cancellationToken);
                    if (origReq != null) origReq.Status = "Completed";
                }
                break;
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("[Maintenance] Updated {Id} to {Status} by user {UserId}",
            request.MaintenanceId, request.NewStatus, request.UserId);
    }
}
