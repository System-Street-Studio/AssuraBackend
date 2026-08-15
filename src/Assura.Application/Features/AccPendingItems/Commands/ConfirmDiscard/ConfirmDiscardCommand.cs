using MediatR;
using Assura.Application.Common.Interfaces;

namespace Assura.Application.Features.AccPendingItems.Commands.ConfirmDiscard;

public class ConfirmDiscardCommand : IRequest<bool>
{
    public int Id { get; set; }
}

public class ConfirmDiscardCommandHandler : IRequestHandler<ConfirmDiscardCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ConfirmDiscardCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ConfirmDiscardCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.AccPendingItems.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null) return false;

        // Remove from pending and add to discarded
        _context.AccPendingItems.Remove(entity);

        var discardedItem = new Domain.Entities.AccDiscardedItem
        {
            Name = entity.Name,
            Division = entity.Division,
            Date = entity.Date,
            AssetType = entity.AssetType,
            CurrentUser = entity.CurrentUser,
            RequestedByName = entity.RequestedByName,
            SpecialNote = entity.SpecialNote,
            ValueAtPurchasing = entity.ValueAtPurchasing,
            CurrentValue = entity.CurrentValue,
            Time = entity.Time
        };

        _context.AccDiscardedItems.Add(discardedItem);

        if (entity.QueueItemId.HasValue)
        {
            var queueItem = await _context.QueueItems.FindAsync(new object[] { entity.QueueItemId.Value }, cancellationToken);
            if (queueItem != null)
            {
                queueItem.Status = Domain.Enums.QueueItemStatus.Discarded;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
