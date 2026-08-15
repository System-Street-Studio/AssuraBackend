using MediatR;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.QueueItems.Commands.UpdateStatus;

public class UpdateQueueItemStatusCommand : IRequest<bool>
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ReviewNote { get; set; }
}

public class UpdateQueueItemStatusCommandValidator : AbstractValidator<UpdateQueueItemStatusCommand>
{
    public UpdateQueueItemStatusCommandValidator()
    {
        RuleFor(x => x.Status)
            .Must(status => Enum.TryParse<QueueItemStatus>(status, true, out _))
            .WithMessage(x => $"'{x.Status}' is not a valid queue item status.");
    }
}

public class UpdateQueueItemStatusCommandHandler : IRequestHandler<UpdateQueueItemStatusCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateQueueItemStatusCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
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
            var actingUserName = await ResolveActingUserNameAsync(cancellationToken);

            var pendingItem = new Domain.Entities.AccPendingItem
            {
                Name = entity.Name,
                Division = entity.Division,
                Date = DateTime.UtcNow,
                Status = "Pending",
                Category = Domain.Enums.AccPendingCategory.Pending,
                AssetType = entity.AssetType,
                CurrentUser = actingUserName,
                SpecialNote = entity.SpecialNote ?? string.Empty,
                ValueAtPurchasing = 0,
                CurrentValue = 0,
                QueueItemId = entity.Id,
                RequestedById = entity.RequestedById,
                RequestedByName = entity.RequestedByName
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
                    Message = $"Asset '{entity.Name}' from {entity.Division} was marked as approved/discarded by {actingUserName}.",
                    UserId = acc.Id,
                    Type = "Info",
                    ReferenceId = pendingItem.Id.ToString()
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<string> ResolveActingUserNameAsync(CancellationToken cancellationToken)
    {
        if (int.TryParse(_currentUserService.UserId, out var actingUserId))
        {
            var actingUser = await _context.Users.FindAsync(new object[] { actingUserId }, cancellationToken);
            if (actingUser != null)
            {
                var fullName = $"{actingUser.FirstName} {actingUser.LastName}".Trim();
                if (!string.IsNullOrEmpty(fullName))
                {
                    return fullName;
                }
            }
        }

        return "Unknown";
    }
}
