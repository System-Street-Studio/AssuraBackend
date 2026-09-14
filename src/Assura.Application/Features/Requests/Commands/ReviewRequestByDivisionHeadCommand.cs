using Assura.Application.Common.Interfaces;
using Assura.Domain.Constants;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Requests.Commands;

public enum ReviewRequestByDivisionHeadResult
{
    Success,
    NotFound,
    Forbidden,
    InvalidStatus
}

public record ReviewRequestByDivisionHeadCommand : IRequest<ReviewRequestByDivisionHeadResult>
{
    public int Id { get; init; }
    public bool Approve { get; init; }
    public string? Remarks { get; init; }
    public int? ReviewedByUserId { get; init; }
    public bool IsAdmin { get; init; }
}

public class ReviewRequestByDivisionHeadCommandHandler : IRequestHandler<ReviewRequestByDivisionHeadCommand, ReviewRequestByDivisionHeadResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IPublisher? _publisher;

    public ReviewRequestByDivisionHeadCommandHandler(IApplicationDbContext context, IPublisher? publisher = null)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<ReviewRequestByDivisionHeadResult> Handle(ReviewRequestByDivisionHeadCommand request, CancellationToken cancellationToken)
    {
        var targetId = Math.Abs(request.Id);

        var entity = await _context.Requests
            .Include(r => r.Requester)
            .Include(r => r.Asset)
            .FirstOrDefaultAsync(r => r.Id == targetId, cancellationToken);

        if (entity == null)
        {
            // Fallback to AssetRequests for legacy/test records
            var assetRequest = await _context.AssetRequests
                .FirstOrDefaultAsync(ar => ar.Id == targetId, cancellationToken);

            if (assetRequest == null)
            {
                return ReviewRequestByDivisionHeadResult.NotFound;
            }

            if (assetRequest.Status != RequestStatus.Pending)
            {
                return ReviewRequestByDivisionHeadResult.InvalidStatus;
            }

            if (!request.IsAdmin)
            {
                var reviewerDivisionId = request.ReviewedByUserId.HasValue
                    ? await _context.Users
                        .Where(u => u.Id == request.ReviewedByUserId.Value)
                        .Select(u => u.DivisionId)
                        .FirstOrDefaultAsync(cancellationToken)
                    : null;

                if (!reviewerDivisionId.HasValue || assetRequest.DivisionId != reviewerDivisionId.Value)
                {
                    return ReviewRequestByDivisionHeadResult.Forbidden;
                }
            }

            if (!request.Approve)
            {
                assetRequest.Status = RequestStatus.Rejected;
                assetRequest.RejectionReason = request.Remarks;

                if (int.TryParse(assetRequest.RequesterId, out var requesterIdVal))
                {
                    _context.Notifications.Add(new Notification
                    {
                        Title = "Asset Request Rejected",
                        Message = $"Your request for '{assetRequest.AssetName}' was rejected by the division head.",
                        UserId = requesterIdVal,
                        Type = "Error",
                        ReferenceId = assetRequest.Id.ToString()
                    });
                }

                await _context.SaveChangesAsync(cancellationToken);
                return ReviewRequestByDivisionHeadResult.Success;
            }

            assetRequest.Status = RequestStatus.Approved;
            await _context.SaveChangesAsync(cancellationToken);

            if (int.TryParse(assetRequest.RequesterId, out var reqId))
            {
                _context.Notifications.Add(new Notification
                {
                    Title = "Asset Request Approved",
                    Message = $"Your asset request ({assetRequest.AssetName}) has been approved.",
                    UserId = reqId,
                    Type = "Success",
                    ReferenceId = assetRequest.Id.ToString()
                });
                await _context.SaveChangesAsync(cancellationToken);
            }

            if (_publisher != null)
            {
                await _publisher.Publish(new AssetRequests.Events.AssetRequestApprovedEvent(
                    assetRequest.Id,
                    assetRequest.AssetName,
                    assetRequest.AssetCategory,
                    assetRequest.Quantity ?? 0,
                    assetRequest.RequestType,
                    assetRequest.Priority,
                    assetRequest.Status.ToString(),
                    assetRequest.RequesterName,
                    assetRequest.RequesterId,
                    "N/A",
                    assetRequest.SubmittedDate,
                    assetRequest.Description ?? "N/A",
                    assetRequest.Reason ?? "N/A",
                    request.ReviewedByUserId
                ), cancellationToken);
            }

            return ReviewRequestByDivisionHeadResult.Success;
        }

        // Only a request still awaiting division-head approval can be decided —
        // otherwise a head could re-approve/re-reject a request another head (or a
        // later stage of the workflow) has already moved past.
        if (entity.Status != RequestWorkflowStatus.PendingDivisionHeadApproval && entity.Status != "Pending")
        {
            return ReviewRequestByDivisionHeadResult.InvalidStatus;
        }

        // Division Heads may only review requests raised within their own division —
        // matches the scoping GetRequestsQueryHandler already applies when listing
        // requests: the requester's division, or (for Transfer requests) the asset's
        // division. Admin bypasses this check.
        if (!request.IsAdmin)
        {
            var reviewerDivisionId = request.ReviewedByUserId.HasValue
                ? await _context.Users
                    .Where(u => u.Id == request.ReviewedByUserId.Value)
                    .Select(u => u.DivisionId)
                    .FirstOrDefaultAsync(cancellationToken)
                : null;

            var inReviewerDivision = reviewerDivisionId.HasValue &&
                (entity.Requester.DivisionId == reviewerDivisionId.Value ||
                 (entity.Type == RequestType.Transfer && entity.Asset != null && entity.Asset.DivisionId == reviewerDivisionId.Value));

            if (!inReviewerDivision)
            {
                return ReviewRequestByDivisionHeadResult.Forbidden;
            }
        }

        entity.DivisionHeadReviewerId = request.ReviewedByUserId;
        entity.DivisionHeadReviewedAt = DateTime.UtcNow;
        entity.Remarks = request.Remarks;

        if (!request.Approve)
        {
            entity.Status = RequestWorkflowStatus.Rejected;

            _context.Notifications.Add(new Notification
            {
                Title = "Request Rejected",
                Message = $"Your request {entity.RequestNumber} was rejected by the division head.",
                UserId = entity.RequesterId,
                Type = "Error",
                ReferenceId = entity.Id.ToString()
            });

            await _context.SaveChangesAsync(cancellationToken);
            return ReviewRequestByDivisionHeadResult.Success;
        }

        entity.Status = RequestWorkflowStatus.PendingStorekeeperReview;

        if (entity.Type == RequestType.Maintenance && entity.AssetId.HasValue)
        {
            var maintenance = new Maintenance
            {
                MaintenanceNumber = "MNT-" + DateTime.Now.ToString("yyyyMMdd") + "-" + entity.Id,
                Type = MaintenanceType.Corrective, // Default
                MaintenanceDate = DateTime.UtcNow,
                Description = entity.Description,
                Cost = 0,
                Status = "Approved",
                Priority = entity.Priority.ToString(),
                RequestedByUserId = entity.RequesterId,
                ApprovedByUserId = request.ReviewedByUserId,
                OriginalRequestId = entity.Id,
                ApprovedAt = DateTime.UtcNow,
                AssetId = entity.AssetId.Value
            };
            _context.Maintenances.Add(maintenance);
        }
        else if (entity.Type == RequestType.Disposal)
        {
            string divisionName = "Unknown";
            if (entity.DivisionId.HasValue)
            {
                divisionName = await _context.Divisions
                    .Where(d => d.Id == entity.DivisionId.Value)
                    .Select(d => d.Name)
                    .FirstOrDefaultAsync(cancellationToken) ?? "Unknown";
            }
            else if (entity.Requester?.DivisionId != null)
            {
                divisionName = await _context.Divisions
                    .Where(d => d.Id == entity.Requester.DivisionId.Value)
                    .Select(d => d.Name)
                    .FirstOrDefaultAsync(cancellationToken) ?? "Unknown";
            }

            var assetName = entity.AssetName ?? entity.Asset?.AssetCode ?? "Asset";
            var requesterName = entity.Requester != null 
                ? $"{entity.Requester.FirstName} {entity.Requester.LastName}" 
                : "Unknown";

            var discardedNote = new DiscardedNote
            {
                Name = assetName,
                Division = divisionName,
                Date = DateTime.UtcNow,
                Status = DiscardNoteStatus.Pending,
                AssetType = entity.AssetCategory ?? "General",
                SpecialNote = entity.Reason ?? entity.Description ?? entity.Remarks ?? "N/A",
                RequestedByUserId = entity.RequesterId,
                RequestedByName = requesterName,
                AssetId = entity.AssetId
            };

            _context.DiscardedNotes.Add(discardedNote);

            var queueItem = new QueueItem
            {
                Name = assetName,
                Division = divisionName,
                Date = DateTime.UtcNow,
                Status = QueueItemStatus.Pending,
                Time = DateTime.UtcNow.TimeOfDay,
                AssetType = entity.AssetCategory ?? "General",
                SpecialNote = entity.Reason ?? entity.Description ?? entity.Remarks ?? "N/A",
                RequestedById = entity.RequesterId.ToString(),
                RequestedByName = requesterName
            };

            _context.QueueItems.Add(queueItem);
            await _context.SaveChangesAsync(cancellationToken);

            discardedNote.QueueItemId = queueItem.Id;

            var superintendents = await _context.Users
                .Where(u => u.Role == UserRole.Superintendent || u.Role == UserRole.Admin)
                .ToListAsync(cancellationToken);

            foreach (var super in superintendents)
            {
                _context.Notifications.Add(new Notification
                {
                    Title = "Discard Request Pending Review",
                    Message = $"Asset '{assetName}' from {divisionName} division is pending your discard review.",
                    UserId = super.Id,
                    Type = "Info",
                    ReferenceId = discardedNote.Id.ToString()
                });
            }
        }

        // Notify the employee (requester) that their request was approved
        _context.Notifications.Add(new Notification
        {
            Title = "Request Approved",
            Message = $"Your request {entity.RequestNumber} was approved by the division head.",
            UserId = entity.RequesterId,
            Type = "Success",
            ReferenceId = entity.Id.ToString()
        });

        var storekeepers = await _context.Users
            .Where(u => u.Role == Domain.Enums.UserRole.Storekeeper || u.Role == Domain.Enums.UserRole.Admin)
            .ToListAsync(cancellationToken);

        foreach (var user in storekeepers)
        {
            _context.Notifications.Add(new Notification
            {
                Title = "Request Ready for Store Verification",
                Message = $"Request {entity.RequestNumber} was approved by division head and is waiting for stock verification.",
                UserId = user.Id,
                Type = "Info",
                ReferenceId = entity.Id.ToString()
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return ReviewRequestByDivisionHeadResult.Success;
    }
}
