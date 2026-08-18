using MediatR;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Assura.Application.Features.AssetRequests.Events;

public class AssetRequestApprovedEventHandler : INotificationHandler<AssetRequestApprovedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AssetRequestApprovedEventHandler> _logger;

    public AssetRequestApprovedEventHandler(IApplicationDbContext context, ILogger<AssetRequestApprovedEventHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(AssetRequestApprovedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // Get the approved request to find its division

            var request = await _context.AssetRequests
                .FirstOrDefaultAsync(x => x.Id == notification.Id, cancellationToken);

            if (request == null)
            {
                return;
            }

            // Handle asset discard request flow
            if (string.Equals(notification.RequestType, "Discard", StringComparison.OrdinalIgnoreCase))
            {
                string divisionName = "Unknown";
                if (request.DivisionId.HasValue)
                {
                    divisionName = await _context.Divisions
                        .Where(d => d.Id == request.DivisionId.Value)
                        .Select(d => d.Name)
                        .FirstOrDefaultAsync(cancellationToken) ?? "Unknown";
                }

                var discardedNote = new DiscardedNote
                {
                    Name = notification.AssetName,
                    Division = divisionName,
                    Date = DateTime.UtcNow,
                    Status = DiscardNoteStatus.Pending,
                    AssetType = notification.AssetCategory,
                    SpecialNote = notification.Reason ?? notification.Description ?? "N/A",
                    RequestedByUserId = int.TryParse(notification.RequesterId, out var requesterIdVal) ? requesterIdVal : null,
                    RequestedByName = notification.RequesterName,
                    AssetId = request.AssetId
                };

                _context.DiscardedNotes.Add(discardedNote);

                // Create a matching QueueItem to display in the Superintendent Overview dashboard
                var queueItem = new QueueItem
                {
                    Name = notification.AssetName,
                    Division = divisionName,
                    Date = DateTime.UtcNow,
                    Status = QueueItemStatus.Pending,
                    Time = DateTime.UtcNow.TimeOfDay,
                    AssetType = notification.AssetCategory,
                    SpecialNote = notification.Reason ?? notification.Description ?? "N/A",
                    RequestedById = notification.RequesterId,
                    RequestedByName = notification.RequesterName
                };

                _context.QueueItems.Add(queueItem);

                // Save to database first to generate the discardedNote.Id and queueItem.Id
                await _context.SaveChangesAsync(cancellationToken);

                // Link the note back to its matching QueueItem so completing it later
                // can carry the link through to the AccPendingItem it spawns.
                discardedNote.QueueItemId = queueItem.Id;

                var superintendents = await _context.Users
                    .Where(u => u.Role == UserRole.Superintendent || u.Role == UserRole.Admin)
                    .ToListAsync(cancellationToken);

                foreach (var super in superintendents)
                {
                    _context.Notifications.Add(new Notification
                    {
                        Title = "Discard Request Pending Review",
                        Message = $"Asset '{notification.AssetName}' from {divisionName} division is pending your discard review.",
                        UserId = super.Id,
                        Type = "Info",
                        ReferenceId = discardedNote.Id.ToString() // Will now have a valid generated ID
                    });
                }

                await _context.SaveChangesAsync(cancellationToken);
                return; // Do not proceed to create AssetInforming
            }

            // Non-discard requests require a division association
            if (!request.DivisionId.HasValue)
            {
                return;
            }

            // Only a genuine new-asset purchase request should create an
            // AssetInforming "new arrival" record — that record means "stock
            // physically arrived and needs registering", which is meaningless for
            // Maintenance/Transfer/other non-purchase request types. Those still
            // need Storekeepers to know the request was approved, just without
            // fabricating a nonexistent arrival (confirmed live via the
            // test-workflow simulation: approving a Maintenance request was
            // creating a bogus "new arrival" entry with no purchase price/model,
            // polluting Procurement's Informed Arrivals queue).
            var isNewAssetPurchase = string.Equals(notification.RequestType, "New Asset", StringComparison.OrdinalIgnoreCase);

            if (isNewAssetPurchase)
            {
                // Notify Storekeepers about the new approved request awaiting procurement.
                // Note: AssetInforming (New Arrival) is only created when goods physically arrive and Procurement informs stores.
                var storekeepers = await _context.Users
                    .Where(u => u.Role == UserRole.Storekeeper)
                    .ToListAsync(cancellationToken);

                foreach (var storekeeper in storekeepers)
                {
                    _context.Notifications.Add(new Notification
                    {
                        Title = "Asset Request Approved",
                        Message = $"Request for '{notification.AssetName}' (Qty: {notification.Quantity}) has been approved and is awaiting procurement.",
                        UserId = storekeeper.Id,
                        Type = "Info",
                        ReferenceId = request.Id.ToString()
                    });
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
            else
            {
                // Maintenance/Transfer/other non-purchase request types: notify
                // Storekeepers the request is approved and ready to process,
                // without creating a fake inventory arrival.
                var storekeepers = await _context.Users
                    .Where(u => u.Role == UserRole.Storekeeper)
                    .ToListAsync(cancellationToken);

                foreach (var storekeeper in storekeepers)
                {
                    _context.Notifications.Add(new Notification
                    {
                        Title = "Asset Request Approved",
                        Message = $"Request '{notification.RequestType}' for '{notification.AssetName}' has been approved and is ready for processing.",
                        UserId = storekeeper.Id,
                        Type = "Info",
                        ReferenceId = request.Id.ToString()
                    });
                }

                // A Maintenance-type AssetRequest needs its own Maintenance record the
                // moment it's approved — Storekeepers only work maintenance items from
                // the dedicated Maintenance queue (GetMaintenancesQuery), and escalating
                // to Procurement (EscalateToProcurementCommand) requires an existing
                // Maintenance row. Without this, a Division-Head-approved Maintenance
                // AssetRequest never appeared anywhere a Storekeeper could act on it.
                // Mirrors the equivalent handling already done for the sibling `Request`
                // entity in ReviewRequestByDivisionHeadCommand.
                if (string.Equals(notification.RequestType, "Maintenance", StringComparison.OrdinalIgnoreCase)
                    && request.AssetId.HasValue)
                {
                    var maintenance = new Maintenance
                    {
                        MaintenanceNumber = "MNT-" + DateTime.Now.ToString("yyyyMMdd") + "-AR" + request.Id,
                        Type = MaintenanceType.Corrective,
                        MaintenanceDate = DateTime.UtcNow,
                        Description = request.Description,
                        Cost = 0,
                        Status = "Approved",
                        Priority = request.Priority,
                        RequestedByUserId = request.UserId,
                        ApprovedByUserId = notification.ApprovedByUserId,
                        OriginalRequestId = request.Id,
                        ApprovedAt = DateTime.UtcNow,
                        AssetId = request.AssetId.Value
                    };
                    _context.Maintenances.Add(maintenance);
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        catch (Exception ex)
        {
            // Log the error but don't throw - we don't want to fail the approval
            _logger.LogError(ex, "Failed to process approved asset request {AssetRequestId} (type: {RequestType})", notification.Id, notification.RequestType);
        }
    }
}

