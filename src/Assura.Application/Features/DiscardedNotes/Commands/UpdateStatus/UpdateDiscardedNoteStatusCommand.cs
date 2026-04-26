using MediatR;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.DiscardedNotes.Commands.UpdateStatus;

public class UpdateDiscardedNoteStatusCommand : IRequest<bool>
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
}

public class UpdateDiscardedNoteStatusCommandHandler : IRequestHandler<UpdateDiscardedNoteStatusCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateDiscardedNoteStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
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

        if (isCompleting)
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
                    Message = $"Asset '{entity.Name}' from {entity.Division} was marked as discarded by Superintendent.",
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
