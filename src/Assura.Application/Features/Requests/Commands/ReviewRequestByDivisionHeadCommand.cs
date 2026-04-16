using Assura.Application.Common.Interfaces;
using Assura.Domain.Constants;
using Assura.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Requests.Commands;

public record ReviewRequestByDivisionHeadCommand : IRequest
{
    public int Id { get; init; }
    public bool Approve { get; init; }
    public string? Remarks { get; init; }
    public int? ReviewedByUserId { get; init; }
}

public class ReviewRequestByDivisionHeadCommandHandler : IRequestHandler<ReviewRequestByDivisionHeadCommand>
{
    private readonly IApplicationDbContext _context;

    public ReviewRequestByDivisionHeadCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ReviewRequestByDivisionHeadCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Requests
            .Include(r => r.Requester)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            return;
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
            return;
        }

        entity.Status = RequestWorkflowStatus.PendingStorekeeperReview;

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
    }
}
