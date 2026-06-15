using MediatR;
using Microsoft.EntityFrameworkCore;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using Assura.Domain.Entities;

namespace Assura.Application.Features.Transfers.Commands;

public record ConfirmTransferByHeadCommand(int TransferId) : IRequest<bool>;

public class ConfirmTransferByHeadCommandHandler : IRequestHandler<ConfirmTransferByHeadCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ConfirmTransferByHeadCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ConfirmTransferByHeadCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _context.Transfers
            .FirstOrDefaultAsync(t => t.Id == request.TransferId, cancellationToken);

        if (transfer == null)
            throw new KeyNotFoundException($"Transfer with ID {request.TransferId} not found");

        // Verify transfer is in the correct status
        if (transfer.Status != TransferStatus.WaitingForFinalConfirmation)
            throw new InvalidOperationException($"Transfer cannot be confirmed from status {transfer.Status}. Expected status: {TransferStatus.WaitingForFinalConfirmation}");


        // Update transfer status
        var asset = await _context.Assets
            .Where(a => a.Id == transfer.AssetId)
            .Select(a => new Asset
            {
                Id = a.Id,
                Status = a.Status,
                AssignedUserId = a.AssignedUserId,
                UpdatedAt = a.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (asset == null)
            throw new KeyNotFoundException($"Asset with ID {transfer.AssetId} not found in the database");

        _context.Assets.Attach(asset);
       

        //update transfer table
        transfer.Status = TransferStatus.Active;
        transfer.UpdatedAt = DateTime.UtcNow;
        transfer.TransferDate = DateTime.UtcNow;

        //update assets table
        asset.Status = AssetStatus.Transferred;
        asset.UpdatedAt = DateTime.UtcNow;

        var result = await _context.SaveChangesAsync(cancellationToken);
        return result > 0;
    }
}
