using MediatR;
using Microsoft.EntityFrameworkCore;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;

namespace Assura.Application.Features.Transfers.Commands;


public record RejectTransferCommand(int TransferId, int UserId) : IRequest<bool>;

public class RejectTransferCommandHandler : IRequestHandler<RejectTransferCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public RejectTransferCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

// Handle the command to reject a transfer
    public async Task<bool> Handle(RejectTransferCommand request, CancellationToken cancellationToken)
    {

        var transfer = await _context.Transfers
            .FirstOrDefaultAsync(t => t.Id == request.TransferId, cancellationToken);

        if (transfer == null)
            throw new Exception("Transfer record not found");

        if (transfer.Status != TransferStatus.PendingOwnerApproval)
            throw new Exception($"Cannot reject transfer in status {transfer.Status}");

        // This is the current holder's counterpart to Accept — declining to hand the
        // asset over. Without this check any authenticated user (confirmed live: an
        // unrelated Storekeeper) could reject a transfer between two other employees
        // in two other divisions. Division Heads reject via the separate
        // RejectTransferByHeadCommand (/reject-head), which already has its own
        // division-scoping check.
        if (transfer.CurrentHolderId != request.UserId)
            throw new UnauthorizedAccessException("Only the asset's current holder may reject this transfer.");

        transfer.Status = TransferStatus.Rejected;
        transfer.UpdatedAt = DateTime.UtcNow;


        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
