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

    public ReviewRequestByDivisionHeadCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReviewRequestByDivisionHeadResult> Handle(ReviewRequestByDivisionHeadCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Requests
            .Include(r => r.Requester)
            .Include(r => r.Asset)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            return ReviewRequestByDivisionHeadResult.NotFound;
        }

        // Only a request still awaiting division-head approval can be decided —
        // otherwise a head could re-approve/re-reject a request another head (or a
        // later stage of the workflow) has already moved past.
        if (entity.Status != RequestWorkflowStatus.PendingDivisionHeadApproval)
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
