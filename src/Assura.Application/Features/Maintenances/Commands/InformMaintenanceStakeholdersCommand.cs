using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Assura.Application.Features.Maintenances.Commands;

public enum InformMaintenanceStakeholdersResult
{
    Success,
    NotFound,
    InvalidStatus
}

public record InformMaintenanceStakeholdersCommand : IRequest<InformMaintenanceStakeholdersResult>
{
    public int MaintenanceId { get; init; }
    public int StorekeeperUserId { get; init; }
}

public class InformMaintenanceStakeholdersCommandHandler : IRequestHandler<InformMaintenanceStakeholdersCommand, InformMaintenanceStakeholdersResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<InformMaintenanceStakeholdersCommandHandler> _logger;

    public InformMaintenanceStakeholdersCommandHandler(IApplicationDbContext context, ILogger<InformMaintenanceStakeholdersCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<InformMaintenanceStakeholdersResult> Handle(InformMaintenanceStakeholdersCommand request, CancellationToken cancellationToken)
    {
        var maintenance = await _context.Maintenances
            .Include(m => m.Asset)
            .Include(m => m.RequestedByUser)
            .FirstOrDefaultAsync(m => m.Id == request.MaintenanceId, cancellationToken);

        if (maintenance == null)
        {
            return InformMaintenanceStakeholdersResult.NotFound;
        }

        // Only a Procurement-completed maintenance can be reported back to the
        // employee/division head — otherwise the Storekeeper could tell them work
        // is done before it actually is.
        if (!string.Equals(maintenance.Status, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            return InformMaintenanceStakeholdersResult.InvalidStatus;
        }

        var assetLabel = maintenance.Asset != null ? maintenance.Asset.AssetCode : $"Asset #{maintenance.AssetId}";

        if (maintenance.RequestedByUserId.HasValue)
        {
            _context.Notifications.Add(new Notification
            {
                Title = "Maintenance Completed",
                Message = $"Maintenance for '{assetLabel}' ({maintenance.MaintenanceNumber}) has been completed and the asset is ready.",
                UserId = maintenance.RequestedByUserId.Value,
                Type = "Success",
                ReferenceId = maintenance.Id.ToString()
            });

            var employeeDivisionId = maintenance.RequestedByUser?.DivisionId;
            if (employeeDivisionId.HasValue)
            {
                var divisionHeads = await _context.Users
                    .Where(u => u.Role == UserRole.DivisionHead && u.DivisionId == employeeDivisionId.Value)
                    .ToListAsync(cancellationToken);

                foreach (var head in divisionHeads)
                {
                    _context.Notifications.Add(new Notification
                    {
                        Title = "Maintenance Completed",
                        Message = $"Maintenance for '{assetLabel}' ({maintenance.MaintenanceNumber}), requested by {maintenance.RequestedByUser!.FirstName} {maintenance.RequestedByUser.LastName}, has been completed.",
                        UserId = head.Id,
                        Type = "Success",
                        ReferenceId = maintenance.Id.ToString()
                    });
                }
            }
        }

        maintenance.Status = "Submitted";
        maintenance.StorekeeperUserId = request.StorekeeperUserId;

        // 1. Re-activate the primary asset and restore its assignment to the requesting employee
        if (maintenance.Asset != null)
        {
            maintenance.Asset.Status = AssetStatus.InUse;
            if (maintenance.RequestedByUserId.HasValue)
            {
                maintenance.Asset.AssignedUserId = maintenance.RequestedByUserId.Value;
            }
        }
        else if (maintenance.AssetId > 0)
        {
            var asset = await _context.Assets.FirstOrDefaultAsync(a => a.Id == maintenance.AssetId, cancellationToken);
            if (asset != null)
            {
                asset.Status = AssetStatus.InUse;
                if (maintenance.RequestedByUserId.HasValue)
                {
                    asset.AssignedUserId = maintenance.RequestedByUserId.Value;
                }
            }
        }

        // 2. Return any temporary replacement asset back to store inventory
        if (maintenance.ReplacementAssetId.HasValue)
        {
            var replacementAsset = await _context.Assets
                .FirstOrDefaultAsync(a => a.Id == maintenance.ReplacementAssetId.Value, cancellationToken);
            if (replacementAsset != null)
            {
                replacementAsset.Status = AssetStatus.InStore;
                replacementAsset.AssignedUserId = null;
            }
        }

        // 3. Mark originating requests as Completed
        if (maintenance.OriginalRequestId.HasValue)
        {
            var originalAssetRequest = await _context.AssetRequests
                .FirstOrDefaultAsync(ar => ar.Id == maintenance.OriginalRequestId.Value, cancellationToken);
            if (originalAssetRequest != null)
            {
                originalAssetRequest.Status = RequestStatus.Completed;
            }

            var originalRequest = await _context.Requests
                .FirstOrDefaultAsync(r => r.Id == maintenance.OriginalRequestId.Value, cancellationToken);
            if (originalRequest != null)
            {
                originalRequest.Status = "Completed";
            }
        }

        // 4. Also mark any pending/approved AssetRequest for this asset as Completed
        if (maintenance.AssetId > 0)
        {
            var relatedAssetRequests = await _context.AssetRequests
                .Where(ar => ar.AssetId == maintenance.AssetId && ar.RequestType == "Maintenance" && ar.Status != RequestStatus.Completed && ar.Status != RequestStatus.Rejected)
                .ToListAsync(cancellationToken);
            foreach (var ar in relatedAssetRequests)
            {
                ar.Status = RequestStatus.Completed;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("[Maintenance] {Id} stakeholders informed, asset reactivated and submitted by storekeeper {UserId}",
            request.MaintenanceId, request.StorekeeperUserId);

        return InformMaintenanceStakeholdersResult.Success;
    }
}
