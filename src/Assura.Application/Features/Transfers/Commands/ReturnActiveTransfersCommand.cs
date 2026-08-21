using MediatR;
using Microsoft.EntityFrameworkCore;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using Assura.Domain.Entities; 
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assura.Application.Features.Transfers.Commands;

public record ReturnActiveTransferCommand(int Id, int CallerId, bool IsAdmin, bool IsDivisionHead) : IRequest<bool>;

public class ReturnActiveTransferCommandHandler : IRequestHandler<ReturnActiveTransferCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ReturnActiveTransferCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ReturnActiveTransferCommand request, CancellationToken cancellationToken)
    {

        var transfer = await _context.Transfers
            .Where(t => t.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (transfer == null)
            throw new Exception($"Transfer with ID {request.Id} not found");

        if (transfer.Status != TransferStatus.Active && transfer.Status != TransferStatus.Overdue)
            throw new Exception($"Transfer cannot be returned from status {transfer.Status}.");

        // Only the employees involved in the transfer (TargetUser or CurrentHolder) or the
        // Division Head of either side of the transfer may return it. Admin bypasses.
        if (!request.IsAdmin)
        {
            if (request.IsDivisionHead)
            {
                var caller = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.CallerId, cancellationToken);
                if (caller?.DivisionId == null ||
                    (caller.DivisionId != transfer.FromDivisionId && caller.DivisionId != transfer.ToDivisionId))
                {
                    throw new UnauthorizedAccessException("You may only return transfers involving your own division.");
                }
            }
            else if (transfer.TargetUserId != request.CallerId && transfer.CurrentHolderId != request.CallerId)
            {
                throw new UnauthorizedAccessException("Only the employees involved in this transfer or a Division Head may return it.");
            }
        }

        var asset = await _context.Assets
            .FirstOrDefaultAsync(a => a.Id == transfer.AssetId, cancellationToken);

        if (asset != null)
        {
            asset.Status = AssetStatus.InUse;
            // Hand the asset back to its original holder — it was reassigned to
            // TargetUserId when the transfer was confirmed (see
            // ConfirmTransferByHeadCommandHandler), and nothing else restores it.
            asset.AssignedUserId = transfer.CurrentHolderId;
            asset.UpdatedAt = DateTime.UtcNow;
        }

        transfer.Status = TransferStatus.Completed;
        transfer.UpdatedAt = DateTime.UtcNow;
        transfer.ReturnDate = DateTime.UtcNow;

        return await _context.SaveChangesAsync(cancellationToken) > 0;
    }
}