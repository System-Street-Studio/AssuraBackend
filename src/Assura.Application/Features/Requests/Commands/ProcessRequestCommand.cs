using Assura.Application.Common.Interfaces;
using Assura.Domain.Constants;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Assura.Domain.Entities;

namespace Assura.Application.Features.Requests.Commands;

public record ProcessRequestCommand : IRequest
{
    public int Id { get; init; }
    public int? AssetId { get; init; }
    public bool IsInStock { get; init; }
    public string? Remarks { get; init; }
    public int? ProcessedByUserId { get; init; }
}

public class ProcessRequestCommandHandler : IRequestHandler<ProcessRequestCommand>
{
    private readonly IApplicationDbContext _context;

    public ProcessRequestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ProcessRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Requests
            .Include(r => r.Requester)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            var assetRequest = await _context.AssetRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (assetRequest == null) return;

            if (request.IsInStock)
            {
                if (!request.AssetId.HasValue) return;

                assetRequest.Status = RequestStatus.TemporaryAssigned;
                assetRequest.AssetId = request.AssetId;
                
                var asset = await _context.Assets
                    .FirstOrDefaultAsync(a => a.Id == request.AssetId.Value, cancellationToken);

                int? requesterIdVal = assetRequest.UserId;
                if (!requesterIdVal.HasValue && int.TryParse(assetRequest.RequesterId, out var rid))
                {
                    requesterIdVal = rid;
                }

                if (asset != null)
                {
                    asset.ReservedForUserId = requesterIdVal;
                    asset.ReservedByRequestId = assetRequest.Id;
                    asset.ReservedUntilUtc = DateTime.UtcNow.AddHours(48);
                }

                _context.Notifications.Add(new Notification
                {
                    Title = "Asset Reserved for Pickup",
                    Message = $"Your request for '{assetRequest.AssetName}' has a temporary reserved asset. Collect it from stores for final confirmation.",
                    UserId = requesterIdVal ?? 0,
                    Type = "Success",
                    ReferenceId = assetRequest.Id.ToString()
                });
            }
            else
            {
                assetRequest.Status = RequestStatus.PendingProcurement;

                var procurementUsers = await _context.Users
                    .Where(u => u.Role == UserRole.Procurement || u.Role == UserRole.Admin)
                    .ToListAsync(cancellationToken);

                foreach (var user in procurementUsers)
                {
                    _context.Notifications.Add(new Notification
                    {
                        Title = "Asset Escalated to Procurement",
                        Message = $"Request for '{assetRequest.AssetName}' could not be fulfilled from stock and requires procurement.",
                        UserId = user.Id,
                        Type = "Warning",
                        ReferenceId = assetRequest.Id.ToString()
                    });
                }

                // Auto-create a Maintenance record when the request type is Maintenance
                if (assetRequest.RequestType == "Maintenance" && assetRequest.AssetId.HasValue)
                {
                    var maintenanceNumber = $"MAINT-{DateTime.UtcNow:yyyyMMdd}-{assetRequest.Id}";
                    _context.Maintenances.Add(new Domain.Entities.Maintenance
                    {
                        MaintenanceNumber = maintenanceNumber,
                        Type = Domain.Enums.MaintenanceType.Corrective,
                        MaintenanceDate = DateTime.UtcNow,
                        Description = assetRequest.Description ?? $"Maintenance required. Raised from Request '{assetRequest.AssetName}'.",
                        Cost = 0,
                        Status = "Pending",
                        AssetId = assetRequest.AssetId.Value
                    });
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        entity.Remarks = request.Remarks;
        entity.StorekeeperProcessorId = request.ProcessedByUserId;
        entity.StorekeeperProcessedAt = DateTime.UtcNow;

        if (request.IsInStock)
        {
            if (!request.AssetId.HasValue)
            {
                return;
            }

            // In stock means reserve temporarily and wait for physical handover confirmation.
            entity.Status = RequestWorkflowStatus.TemporaryAssigned;
            entity.AssetId = request.AssetId;
            entity.TemporarilyAssignedAt = DateTime.UtcNow;

            var asset = await _context.Assets
                .FirstOrDefaultAsync(a => a.Id == request.AssetId.Value, cancellationToken);

            if (asset != null)
            {
                asset.ReservedForUserId = entity.RequesterId;
                asset.ReservedByRequestId = entity.Id;
                asset.ReservedUntilUtc = DateTime.UtcNow.AddHours(48);
            }

            // Notify employee that reserved asset is ready for pickup.
            _context.Notifications.Add(new Notification
            {
                Title = "Asset Reserved for Pickup",
                Message = $"Your request {entity.RequestNumber} has a temporary reserved asset. Collect it from stores for final confirmation.",
                UserId = entity.RequesterId,
                Type = "Success",
                ReferenceId = entity.Id.ToString()
            });

            // 2. Notify Division Head
            if (entity.Requester.DivisionId.HasValue)
            {
                var divisionHeads = await _context.Users
                    .Where(u => u.DivisionId == entity.Requester.DivisionId && u.Role == UserRole.DivisionHead)
                    .ToListAsync(cancellationToken);

                foreach (var head in divisionHeads)
                {
                    _context.Notifications.Add(new Notification
                    {
                        Title = "Temporary Asset Assigned",
                        Message = $"Request {entity.RequestNumber} has a temporary asset reservation and is awaiting pickup confirmation.",
                        UserId = head.Id,
                        Type = "Info",
                        ReferenceId = entity.Id.ToString()
                    });
                }
            }
        }
        else
        {
            // Flow: Not Found -> Notify Procurement
            entity.Status = RequestWorkflowStatus.PendingProcurement;

            var procurementUsers = await _context.Users
                .Where(u => u.Role == UserRole.Procurement || u.Role == UserRole.Admin)
                .ToListAsync(cancellationToken);

            foreach (var user in procurementUsers)
            {
                _context.Notifications.Add(new Notification
                {
                    Title = "Asset Escalated to Procurement",
                    Message = $"Request {entity.RequestNumber} could not be fulfilled from stock and requires procurement.",
                    UserId = user.Id,
                    Type = "Warning",
                    ReferenceId = entity.Id.ToString()
                });
            }

            // Auto-create a Maintenance record when the request type is Maintenance
            if (entity.Type == Domain.Enums.RequestType.Maintenance && entity.AssetId.HasValue)
            {
                var maintenanceNumber = $"MAINT-{DateTime.UtcNow:yyyyMMdd}-{entity.Id}";
                _context.Maintenances.Add(new Domain.Entities.Maintenance
                {
                    MaintenanceNumber = maintenanceNumber,
                    Type = Domain.Enums.MaintenanceType.Corrective,
                    MaintenanceDate = DateTime.UtcNow,
                    Description = entity.Description ?? $"Maintenance required. Raised from Request {entity.RequestNumber}.",
                    Cost = 0,
                    Status = "Pending",
                    AssetId = entity.AssetId.Value
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
