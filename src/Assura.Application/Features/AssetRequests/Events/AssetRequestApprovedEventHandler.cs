using MediatR;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.AssetRequests.Events;

public class AssetRequestApprovedEventHandler : INotificationHandler<AssetRequestApprovedEvent>
{
    private readonly IApplicationDbContext _context;

    public AssetRequestApprovedEventHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(AssetRequestApprovedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // Get the approved request to find its division
            var request = await _context.AssetRequests
                .FirstOrDefaultAsync(x => x.Id == notification.Id, cancellationToken);

            if (request?.DivisionId == null)
                return;

            if (string.Equals(notification.RequestType, "Discard", StringComparison.OrdinalIgnoreCase))
            {
                var divisionName = await _context.Divisions
                    .Where(d => d.Id == request.DivisionId.Value)
                    .Select(d => d.Name)
                    .FirstOrDefaultAsync(cancellationToken) ?? "Unknown";

                var discardedNote = new DiscardedNote
                {
                    Name = notification.AssetName,
                    Division = divisionName,
                    Date = DateTime.UtcNow,
                    Status = DiscardNoteStatus.Pending,
                    AssetType = notification.AssetCategory,
                    SpecialNote = notification.Reason ?? notification.Description ?? "N/A"
                };

                _context.DiscardedNotes.Add(discardedNote);

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
                        ReferenceId = discardedNote.Id.ToString()
                    });
                }

                await _context.SaveChangesAsync(cancellationToken);
                return; // Do not proceed to create AssetInforming
            }

            // Create AssetInforming record (adds to inventory/new arrivals)
            var assetInforming = new AssetInforming
            {
                ItemName = notification.AssetName,
                Model = notification.AssetCategory,
                Warranty = "N/A",
                Quantity = notification.Quantity,
                PurchasedDate = notification.SubmittedDate,
                PurchasedPrice = 0, // Can be updated later
                DivisionId = request.DivisionId.Value,
                Status = "Pending"
            };

            _context.AssetInformings.Add(assetInforming);
            await _context.SaveChangesAsync(cancellationToken);

            // Notify Storekeepers about the new approved request
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
                    ReferenceId = assetInforming.Id.ToString()
                });
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Log the error but don't throw - we don't want to fail the approval
            Console.WriteLine($"[ERROR] Failed to create AssetInforming for approved request {notification.Id}: {ex.Message}");
        }
    }
}
