using MediatR;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;

namespace Assura.Application.Features.Transfers.Handlers;

public class RejectTransferByHeadCommandHandler : IRequestHandler<Commands.RejectTransferByHeadCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public RejectTransferByHeadCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(Commands.RejectTransferByHeadCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _context.Transfers.FindAsync(new object[] { request.TransferId }, cancellationToken);

        if (transfer == null)
        {
            throw new Exception($"Transfer {request.TransferId} not found");
        }

        // Allow rejection if pending any approval
        if (transfer.Status != TransferStatus.PendingOwnerDivisionHeadApproval && 
            transfer.Status != TransferStatus.PendingOwnerApproval &&
            transfer.Status != TransferStatus.WaitingForFinalConfirmation)
        {
            throw new Exception($"Cannot reject transfer in status {transfer.Status}");
        }

        // Update status to Rejected
        transfer.Status = TransferStatus.Rejected;
        
        // Append rejection reason
        transfer.Reason = string.IsNullOrEmpty(transfer.Reason) 
            ? $"Rejected: {request.Reason}" 
            : $"{transfer.Reason} | Rejected: {request.Reason}";

        transfer.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
