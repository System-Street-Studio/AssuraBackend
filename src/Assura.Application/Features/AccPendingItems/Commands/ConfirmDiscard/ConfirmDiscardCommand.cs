using MediatR;
using FluentValidation;
using Assura.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.AccPendingItems.Commands.ConfirmDiscard;

public class ConfirmDiscardCommand : IRequest<bool>
{
    public int Id { get; set; }
    public int ReceiptId { get; set; }
}

public class ConfirmDiscardCommandValidator : AbstractValidator<ConfirmDiscardCommand>
{
    public ConfirmDiscardCommandValidator()
    {
        RuleFor(x => x.ReceiptId)
            .GreaterThan(0)
            .WithMessage("A receipt must be attached before confirming this discard.");
    }
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

        // The receipt must exist and actually have an uploaded file — a bare receipt
        // record with no attachment doesn't satisfy "attach a receipt" in spirit.
        var receipt = await _context.Receipts
            .FirstOrDefaultAsync(r => r.Id == request.ReceiptId, cancellationToken);

        if (receipt == null || string.IsNullOrEmpty(receipt.FileUrl))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.ReceiptId), "Receipt not found or has no uploaded file.")
            });
        }

        if (entity.SoldPrice.HasValue && entity.SoldPrice.Value >= 0)
        {
            receipt.Amount = entity.SoldPrice.Value;
        }

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
            Time = entity.Time,
            ReceiptId = request.ReceiptId,
            BuyerId = entity.BuyerId,
            SoldPrice = entity.SoldPrice
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

        if (entity.AssetId.HasValue)
        {
            var asset = await _context.Assets.FindAsync(new object[] { entity.AssetId.Value }, cancellationToken);
            if (asset != null)
            {
                asset.Status = Domain.Enums.AssetStatus.Discarded;
                asset.AssignedUserId = null;
            }
        }

        var matchingDiscardNote = await _context.DiscardedNotes
            .FirstOrDefaultAsync(d => (entity.QueueItemId.HasValue && d.QueueItemId == entity.QueueItemId.Value) ||
                                      (entity.AssetId.HasValue && d.AssetId == entity.AssetId.Value), cancellationToken);
        if (matchingDiscardNote != null)
        {
            matchingDiscardNote.Status = Domain.Enums.DiscardNoteStatus.Completed;
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Link the buyer the Superintendent assigned back at approval time to the now-created
        // AccDiscardedItem — a second save since discardedItem.Id is only known after the first.
        if (entity.BuyerId.HasValue)
        {
            var buyer = await _context.Buyers.FindAsync(new object[] { entity.BuyerId.Value }, cancellationToken);
            if (buyer != null)
            {
                buyer.AccDiscardedItemId = discardedItem.Id;
                buyer.Status = Domain.Enums.BuyerStatus.Sold;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        return true;
    }
}
