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
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Valid Queue Item ID is required.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(status => Enum.TryParse<QueueItemStatus>(status, true, out _))
            .WithMessage(x => $"'{x.Status}' is not a valid queue item status.");

        RuleFor(x => x.ReviewNote)
            .MaximumLength(1000).WithMessage("Review note cannot exceed 1000 characters.");
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

        var matchingNote = await _context.DiscardedNotes
            .FirstOrDefaultAsync(d => d.QueueItemId == entity.Id || (d.Name == entity.Name && d.Division == entity.Division && d.Status == DiscardNoteStatus.Pending), cancellationToken);

        if (matchingNote != null)
        {
            if (isApproving)
            {
                matchingNote.Status = DiscardNoteStatus.Completed;
            }
            else if (string.Equals(request.Status, "Rejected", StringComparison.OrdinalIgnoreCase))
            {
                matchingNote.Status = DiscardNoteStatus.Rejected;
            }

            if (!string.IsNullOrEmpty(request.ReviewNote))
            {
                matchingNote.SpecialNote = request.ReviewNote;
            }
        }

        if (isApproving)
        {
            var actingUserName = await ResolveActingUserNameAsync(cancellationToken);

            int? assetId = matchingNote?.AssetId;
            decimal purchasePrice = 0;
            decimal currentValue = 0;

            if (assetId.HasValue)
            {
                var asset = await _context.Assets.FindAsync(new object[] { assetId.Value }, cancellationToken);
                if (asset != null)
                {
                    purchasePrice = asset.PurchaseValue;
                    currentValue = asset.PurchaseValue;
                }
            }

            var pendingItem = new Domain.Entities.AccPendingItem
            {
                Name = entity.Name,
                Division = entity.Division,
                Date = DateTime.UtcNow,
                Status = "Approved",
                Category = Domain.Enums.AccPendingCategory.Approved,
                AssetType = entity.AssetType,
                CurrentUser = actingUserName,
                SpecialNote = entity.SpecialNote ?? string.Empty,
                ValueAtPurchasing = purchasePrice,
                CurrentValue = currentValue,
                AssetId = assetId,
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
