using Assura.Application.Common.Interfaces;
using Assura.Domain.Constants;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Assura.Application.Features.Requests.Commands;

public record ConfirmTemporaryAssignmentCommand : IRequest
{
    public int Id { get; init; }
    public int? ConfirmedByUserId { get; init; }
    public string? Remarks { get; init; }

    // Set by the controller from the caller's JWT role claim — defense-in-depth, matching
    // ProcessRequestCommand.CallerRole; see that command's comment for why.
    public string? CallerRole { get; init; }
}

public class ConfirmTemporaryAssignmentCommandHandler : IRequestHandler<ConfirmTemporaryAssignmentCommand>
{
    private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        Roles.Storekeeper, Roles.Admin
    };

    private readonly IApplicationDbContext _context;

    public ConfirmTemporaryAssignmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ConfirmTemporaryAssignmentCommand request, CancellationToken cancellationToken)
    {
        if (request.CallerRole == null || !AllowedRoles.Contains(request.CallerRole))
        {
            throw new UnauthorizedAccessException("Only Storekeeper or Admin may confirm a temporary assignment.");
        }

        // Negative ID means this is an AssetRequest record (from unified list)
        if (request.Id < 0)
        {
            var actualId = Math.Abs(request.Id);
            var assetRequest = await _context.AssetRequests
                .Include(r => r.Asset)
                .FirstOrDefaultAsync(r => r.Id == actualId, cancellationToken);

            if (assetRequest == null || assetRequest.Asset == null)
            {
                return;
            }

            if (assetRequest.Status != RequestStatus.TemporaryAssigned)
            {
                return;
            }

            assetRequest.Status = RequestStatus.Approved;
            if (!string.IsNullOrEmpty(request.Remarks))
            {
                assetRequest.Reason = (assetRequest.Reason ?? "") + " (Remarks: " + request.Remarks + ")";
            }

            int? requesterIdVal = assetRequest.UserId;
            if (!requesterIdVal.HasValue && int.TryParse(assetRequest.RequesterId, out var rid))
            {
                requesterIdVal = rid;
            }

            assetRequest.Asset.ReservedForUserId = null;
            assetRequest.Asset.ReservedByRequestId = null;
            assetRequest.Asset.ReservedUntilUtc = null;
            assetRequest.Asset.AssignedUserId = requesterIdVal;
            assetRequest.Asset.Status = AssetStatus.InUse;

            _context.Notifications.Add(new Domain.Entities.Notification
            {
                Title = "Asset Assignment Confirmed",
                Message = $"Your request for '{assetRequest.AssetName}' is fully confirmed. The asset is now assigned to you.",
                UserId = requesterIdVal ?? 0,
                Type = "Success",
                ReferenceId = assetRequest.Id.ToString()
            });

            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        var entity = await _context.Requests
            .Include(r => r.Asset)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            var assetRequest = await _context.AssetRequests
                .Include(r => r.Asset)
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (assetRequest == null || assetRequest.Asset == null)
            {
                return;
            }

            if (assetRequest.Status != RequestStatus.TemporaryAssigned)
            {
                return;
            }

            assetRequest.Status = RequestStatus.Approved;
            if (!string.IsNullOrEmpty(request.Remarks))
            {
                assetRequest.Reason = (assetRequest.Reason ?? "") + " (Remarks: " + request.Remarks + ")";
            }

            int? requesterIdVal = assetRequest.UserId;
            if (!requesterIdVal.HasValue && int.TryParse(assetRequest.RequesterId, out var rid))
            {
                requesterIdVal = rid;
            }

            assetRequest.Asset.ReservedForUserId = null;
            assetRequest.Asset.ReservedByRequestId = null;
            assetRequest.Asset.ReservedUntilUtc = null;
            assetRequest.Asset.AssignedUserId = requesterIdVal;
            assetRequest.Asset.Status = AssetStatus.InUse;

            _context.Notifications.Add(new Domain.Entities.Notification
            {
                Title = "Asset Assignment Confirmed",
                Message = $"Your request for '{assetRequest.AssetName}' is fully confirmed. The asset is now assigned to you.",
                UserId = requesterIdVal ?? 0,
                Type = "Success",
                ReferenceId = assetRequest.Id.ToString()
            });

            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        if (entity.Asset == null)
        {
            return;
        }

        if (!string.Equals(entity.Status, RequestWorkflowStatus.TemporaryAssigned, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Lookup who confirmed (storekeeper name)
        string confirmedByName = "Storekeeper";
        if (request.ConfirmedByUserId.HasValue)
        {
            var actor = await _context.Users.FindAsync([request.ConfirmedByUserId.Value], cancellationToken);
            if (actor != null)
                confirmedByName = $"{actor.FirstName} {actor.LastName}".Trim();
        }

        // Build checkout metadata so the checkout page can display this correctly
        var meta = new
        {
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)), // default 30-day due date
            CheckedOutBy = confirmedByName
        };
        entity.Remarks = JsonSerializer.Serialize(meta);

        // Mark as Checked Out — this makes it appear on the Checkout Page
        entity.Status = RequestWorkflowStatus.CheckedOut;
        entity.PickupConfirmedAt = DateTime.UtcNow;

        entity.Asset.ReservedForUserId = null;
        entity.Asset.ReservedByRequestId = null;
        entity.Asset.ReservedUntilUtc = null;
        entity.Asset.AssignedUserId = entity.RequesterId;
        entity.Asset.Status = AssetStatus.InUse;

        _context.Notifications.Add(new Domain.Entities.Notification
        {
            Title = "Asset Assignment Confirmed",
            Message = $"Your request {entity.RequestNumber} is fully confirmed. The asset is now assigned to you.",
            UserId = entity.RequesterId,
            Type = "Success",
            ReferenceId = entity.Id.ToString()
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
