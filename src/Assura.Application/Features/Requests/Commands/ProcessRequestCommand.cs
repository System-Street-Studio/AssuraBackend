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

    // Set by the controller from the caller's JWT role claim — defense-in-depth so this
    // handler doesn't rely solely on RequestsController's [Authorize(Roles=...)] to keep out
    // a caller a future direct MediatR invocation (another handler, a background job) might
    // not go through the controller at all.
    public string? CallerRole { get; init; }
}

public class ProcessRequestCommandHandler : IRequestHandler<ProcessRequestCommand>
{
    private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        Roles.Storekeeper, Roles.Admin, Roles.Procurement
    };

    private readonly IApplicationDbContext _context;

    public ProcessRequestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ProcessRequestCommand request, CancellationToken cancellationToken)
    {
        if (request.CallerRole == null || !AllowedRoles.Contains(request.CallerRole))
        {
            throw new UnauthorizedAccessException("Only Storekeeper, Procurement, or Admin may process a request.");
        }

        // Negative ID means this is an AssetRequest record (from unified list)
        if (request.Id < 0)
        {
            var actualId = Math.Abs(request.Id);
            var assetRequest = await _context.AssetRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == actualId, cancellationToken);

            if (assetRequest == null) return;
            if (assetRequest.Status != RequestStatus.PendingStorekeeperReview) return;

            await ProcessAssetRequest(assetRequest, request, cancellationToken);
            return;
        }

        var entity = await _context.Requests
            .Include(r => r.Requester)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            var assetRequest = await _context.AssetRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (assetRequest == null) return;
            if (assetRequest.Status != RequestStatus.PendingStorekeeperReview) return;

            await ProcessAssetRequest(assetRequest, request, cancellationToken);
            return;
        }

        // Reprocessing an already-processed request (e.g. a duplicate/retried call) must be a
        // no-op, not a second reservation of a possibly different asset against the same request.
        if (entity.Status != RequestWorkflowStatus.PendingStorekeeperReview) return;

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

    private async Task ProcessAssetRequest(AssetRequest assetRequest, ProcessRequestCommand request, CancellationToken cancellationToken)
    {
        assetRequest.ProcessorRemarks = request.Remarks;
        assetRequest.ProcessedByUserId = request.ProcessedByUserId;
        assetRequest.ProcessedAt = DateTime.UtcNow;

        if (request.ProcessedByUserId.HasValue)
        {
            var processor = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.ProcessedByUserId.Value, cancellationToken);

            if (processor != null)
            {
                assetRequest.ProcessedByName = $"{processor.FirstName} {processor.LastName}";
            }
        }

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

            // No Maintenance record is created here anymore: for Maintenance-type
            // AssetRequests, AssetRequestApprovedEventHandler already creates it the
            // moment the Division Head approves the request (so Storekeepers can see
            // and act on it immediately from the Maintenance queue). Creating a second
            // one here would leave a duplicate, orphaned Maintenance row behind.
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
