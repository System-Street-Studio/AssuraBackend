using MediatR;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using Assura.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.AssetRequests.Commands;

public enum RejectAssetRequestResult
{
    Success,
    NotFound,
    Forbidden,
    InvalidStatus
}

public record RejectAssetRequestCommand(int Id, int UserId, bool IsAdmin, string? Reason = null) : IRequest<RejectAssetRequestResult>;

public class RejectAssetRequestHandler : IRequestHandler<RejectAssetRequestCommand, RejectAssetRequestResult>
{
    private readonly IApplicationDbContext _context;
    public RejectAssetRequestHandler(IApplicationDbContext context) => _context = context;

    public async Task<RejectAssetRequestResult> Handle(RejectAssetRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.AssetRequests.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null) return RejectAssetRequestResult.NotFound;

        // Only a still-pending request can be rejected — see ApproveAssetRequestHandler
        // for why re-deciding an already-resolved request is disallowed.
        if (entity.Status != RequestStatus.Pending) return RejectAssetRequestResult.InvalidStatus;

        // Division Heads may only act on requests raised within their own division;
        // Admin bypasses this scoping check.
        if (!request.IsAdmin)
        {
            var caller = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            if (caller?.DivisionId == null || entity.DivisionId == null || caller.DivisionId != entity.DivisionId)
            {
                return RejectAssetRequestResult.Forbidden;
            }
        }

        entity.Status = RequestStatus.Rejected;
        entity.RejectionReason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason;
        await _context.SaveChangesAsync(cancellationToken);

        // Notify requester
        if (int.TryParse(entity.RequesterId, out var requesterId))
        {
            var message = string.IsNullOrWhiteSpace(entity.RejectionReason)
                ? $"Your asset request ({entity.AssetName}) has been rejected."
                : $"Your asset request ({entity.AssetName}) has been rejected. Reason: {entity.RejectionReason}";

            _context.Notifications.Add(new Notification
            {
                Title = "Asset Request Rejected",
                Message = message,
                UserId = requesterId,
                Type = "Error",
                ReferenceId = entity.Id.ToString()
            });
            await _context.SaveChangesAsync(cancellationToken);
        }
        return RejectAssetRequestResult.Success;
    }
}
