using MediatR;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.DiscardedNotes.Commands.UpdateStatus;

public class UpdateDiscardedNoteStatusCommand : IRequest<bool>
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
}

public class UpdateDiscardedNoteStatusCommandValidator : AbstractValidator<UpdateDiscardedNoteStatusCommand>
{
    public UpdateDiscardedNoteStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Valid Discarded Note ID is required.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(status => Enum.TryParse<DiscardNoteStatus>(status, true, out _))
            .WithMessage(x => $"'{x.Status}' is not a valid discarded note status.");

        RuleFor(x => x.Note)
            .MaximumLength(1000).WithMessage("Note cannot exceed 1000 characters.");
    }
}

public class UpdateDiscardedNoteStatusCommandHandler : IRequestHandler<UpdateDiscardedNoteStatusCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateDiscardedNoteStatusCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(UpdateDiscardedNoteStatusCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.DiscardedNotes.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null) return false;

        bool isCompleting = false;

        if (Enum.TryParse<DiscardNoteStatus>(request.Status, true, out var status))
        {
            if (entity.Status != DiscardNoteStatus.Completed && status == DiscardNoteStatus.Completed)
            {
                isCompleting = true;
            }
            entity.Status = status;
        }

        if (!string.IsNullOrEmpty(request.Note))
        {
            entity.SpecialNote = request.Note;
        }

        if (entity.QueueItemId.HasValue && status == DiscardNoteStatus.Rejected)
        {
            var queueItem = await _context.QueueItems.FindAsync(new object[] { entity.QueueItemId.Value }, cancellationToken);
            if (queueItem != null)
            {
                queueItem.Status = QueueItemStatus.Rejected;
                if (!string.IsNullOrEmpty(request.Note))
                {
                    queueItem.ReviewNote = request.Note;
                }
            }
        }

        if (isCompleting)
        {
            var actingUserName = await ResolveActingUserNameAsync(cancellationToken);

            decimal purchasePrice = 0;
            decimal currentValue = 0;

            if (entity.AssetId.HasValue)
            {
                var asset = await _context.Assets.FindAsync(new object[] { entity.AssetId.Value }, cancellationToken);
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
                Status = "Pending",
                Category = Domain.Enums.AccPendingCategory.Pending,
                AssetType = entity.AssetType,
                CurrentUser = actingUserName,
                SpecialNote = entity.SpecialNote ?? string.Empty,
                ValueAtPurchasing = purchasePrice,
                CurrentValue = currentValue,
                AssetId = entity.AssetId,
                QueueItemId = entity.QueueItemId,
                RequestedById = entity.RequestedByUserId?.ToString(),
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
                    Message = $"Asset '{entity.Name}' from {entity.Division} was marked as discarded by {actingUserName}.",
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
