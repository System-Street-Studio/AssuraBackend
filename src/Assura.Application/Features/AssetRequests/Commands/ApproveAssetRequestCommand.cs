using MediatR;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Constants;
using Assura.Domain.Enums;
using Assura.Application.Features.AssetRequests.Events;
using Assura.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Assura.Application.Features.AssetRequests.Commands;

public enum ApproveAssetRequestResult
{
    Success,
    NotFound,
    Forbidden,
    InvalidStatus
}

public record ApproveAssetRequestCommand(int Id, int UserId, bool IsAdmin) : IRequest<ApproveAssetRequestResult>;

public class ApproveAssetRequestHandler : IRequestHandler<ApproveAssetRequestCommand, ApproveAssetRequestResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IPublisher _publisher;
    public ApproveAssetRequestHandler(IApplicationDbContext context, IPublisher publisher)
    {

     _context = context;
     _publisher = publisher;
    }

    public async Task<ApproveAssetRequestResult> Handle(ApproveAssetRequestCommand request, CancellationToken cancellationToken)
    {
        var targetId = Math.Abs(request.Id);
        var entity = await _context.AssetRequests.FindAsync(new object[] { targetId }, cancellationToken);

        if (entity == null)
        {
            var reqEntity = await _context.Requests.Include(r => r.Requester).FirstOrDefaultAsync(r => r.Id == targetId, cancellationToken);
            if (reqEntity == null) return ApproveAssetRequestResult.NotFound;

            if (reqEntity.Status != RequestWorkflowStatus.PendingDivisionHeadApproval && reqEntity.Status != "Pending")
            {
                return ApproveAssetRequestResult.InvalidStatus;
            }

            if (!request.IsAdmin)
            {
                var caller = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
                var reqDivisionId = reqEntity.DivisionId ?? reqEntity.Requester?.DivisionId;
                if (caller?.DivisionId == null || reqDivisionId == null || caller.DivisionId != reqDivisionId)
                {
                    return ApproveAssetRequestResult.Forbidden;
                }
            }

            reqEntity.Status = RequestWorkflowStatus.PendingStorekeeperReview;
            reqEntity.DivisionHeadReviewerId = request.UserId;
            reqEntity.DivisionHeadReviewedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            _context.Notifications.Add(new Notification
            {
                Title = "Request Approved",
                Message = $"Your request {reqEntity.RequestNumber} has been approved by the division head.",
                UserId = reqEntity.RequesterId,
                Type = "Success",
                ReferenceId = reqEntity.Id.ToString()
            });
            await _context.SaveChangesAsync(cancellationToken);

            return ApproveAssetRequestResult.Success;
        }

        // Only a still-pending request can be approved — otherwise a Division Head
        // could re-approve/flip a request another head or the requester has already
        // moved on from (rejected/cancelled/already approved).
        if (entity.Status != RequestStatus.Pending) return ApproveAssetRequestResult.InvalidStatus;

        // Division Heads may only act on requests raised within their own division;
        // Admin bypasses this scoping check.
        if (!request.IsAdmin)
        {
            var caller = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            if (caller?.DivisionId == null || entity.DivisionId == null || caller.DivisionId != entity.DivisionId)
            {
                return ApproveAssetRequestResult.Forbidden;
            }
        }

        entity.Status = RequestStatus.Approved; // status  change
        await _context.SaveChangesAsync(cancellationToken);

        // Notify requester
        if (int.TryParse(entity.RequesterId, out var requesterId))
        {
            _context.Notifications.Add(new Notification
            {
                Title = "Asset Request Approved",
                Message = $"Your asset request ({entity.AssetName}) has been approved.",
                UserId = requesterId,
                Type = "Success",
                ReferenceId = entity.Id.ToString()
            });
            await _context.SaveChangesAsync(cancellationToken);
        }

        await _publisher.Publish(new AssetRequestApprovedEvent(
            entity.Id ,
            entity.AssetName,
            entity.AssetCategory,
            entity.Quantity ?? 0,
            entity.RequestType,
            entity.Priority,
            entity.Status .ToString(),
            entity.RequesterName,
            entity.RequesterId,
            entity.Attachments?.Count > 0
                ? JsonSerializer.Serialize(entity.Attachments.Select(a => new { a.FileName, a.FileUrl }).ToList())
                : "N/A",
            entity.SubmittedDate,
            entity.Description ?? "N/A",
            entity.Reason ?? "N/A",
            request.UserId
        ), cancellationToken);

        return ApproveAssetRequestResult.Success;
    }
}
