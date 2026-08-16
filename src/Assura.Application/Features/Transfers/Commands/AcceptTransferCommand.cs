using MediatR;
using Microsoft.EntityFrameworkCore;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;

namespace Assura.Application.Features.Transfers.Commands;


public record AcceptTransferCommand(int TransferId, int UserId) : IRequest<bool>;

public class AcceptTransferCommandHandler : IRequestHandler<AcceptTransferCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public AcceptTransferCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
// Handle the command to accept a transfer
    public async Task<bool> Handle(AcceptTransferCommand request, CancellationToken cancellationToken)
    {

        var transfer = await _context.Transfers
            .FirstOrDefaultAsync(t => t.Id == request.TransferId, cancellationToken);

        if (transfer == null)
            throw new Exception("Transfer record not found");

        if (transfer.Status != TransferStatus.PendingOwnerApproval)
            throw new Exception($"Cannot accept transfer in status {transfer.Status}");

        // Only the current holder of the asset — the person this transfer actually
        // asks to hand it over — may accept it. Without this check any authenticated
        // user (confirmed live: another employee, even a Storekeeper unrelated to the
        // transfer) could accept a transfer they have no part in.
        if (transfer.CurrentHolderId != request.UserId)
            throw new UnauthorizedAccessException("Only the asset's current holder may accept this transfer.");

        transfer.Status = TransferStatus.PendingOwnerDivisionHeadApproval;
        transfer.UpdatedAt = DateTime.UtcNow;


        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
