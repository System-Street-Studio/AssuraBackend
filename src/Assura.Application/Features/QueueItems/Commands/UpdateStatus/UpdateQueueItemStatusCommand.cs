using MediatR;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.QueueItems.Commands.UpdateStatus;

public class UpdateQueueItemStatusCommand : IRequest<bool>
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ReviewNote { get; set; }
}

public class UpdateQueueItemStatusCommandHandler : IRequestHandler<UpdateQueueItemStatusCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateQueueItemStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateQueueItemStatusCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.QueueItems.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null) return false;

        bool isApproving = false;

        if (Enum.TryParse<QueueItemStatus>(request.Status, true, out var status))
        {
            if (entity.Status != QueueItemStatus.Approved && entity.Status != QueueItemStatus.Discarded && 
                (status == QueueItemStatus.Approved || status == QueueItemStatus.Discarded))
            {
                isApproving = true;
            }
            entity.Status = status;
        }

        if (!string.IsNullOrEmpty(request.ReviewNote))
        {
            entity.ReviewNote = request.ReviewNote;
        }

        if (isApproving)
        {
            var pendingItem = new Domain.Entities.AccPendingItem
            {
                Name = entity.Name,
                Division = entity.Division,
                Date = DateTime.UtcNow,
                Status = "Pending",
                Category = Domain.Enums.AccPendingCategory.Pending,
                AssetType = entity.AssetType,
                CurrentUser = "Superintendent",
                SpecialNote = entity.SpecialNote ?? string.Empty,
                ValueAtPurchasing = 0,
                CurrentValue = 0
            };

            _context.AccPendingItems.Add(pendingItem);

            var accountants = await _context.Users
                .Where(u => u.Role == Domain.Enums.UserRole.Accountant || u.Role == Domain.Enums.UserRole.Admin)
                .ToListAsync(cancellationToken);

            foreach (var acc in accountants)
            {
                _context.Notifications.Add(new Domain.Entities.Notification
                {
                    Title = "New Discard Confirmation Needed",
                    Message = $"Asset '{entity.Name}' from {entity.Division} was marked as approved/discarded by Superintendent.",
                    UserId = acc.Id,
                    Type = "Info",
                    ReferenceId = pendingItem.Id.ToString()
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
