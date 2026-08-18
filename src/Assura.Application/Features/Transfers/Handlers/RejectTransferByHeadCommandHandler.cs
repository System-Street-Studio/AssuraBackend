using MediatR;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using Microsoft.EntityFrameworkCore;

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

        // Which division's head may reject depends on which approval stage the
        // transfer is waiting at — mirrors GetDivisionHeadTransferQueryHandler's
        // "incoming"/"outgoing"/"pending" tab scoping for the same statuses.
        var expectedDivisionId = transfer.Status == TransferStatus.PendingOwnerDivisionHeadApproval
            ? transfer.FromDivisionId
            : transfer.ToDivisionId;

        var caller = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.DivisionHeadId, cancellationToken);
        if (caller?.DivisionId == null || caller.DivisionId != expectedDivisionId)
        {
            throw new UnauthorizedAccessException("You may only reject transfers pending your own division's action.");
        }

        var oldStatus = transfer.Status;
        // Update status to Rejected
        transfer.Status = TransferStatus.RejectedByDivisionHead;
        
        // Append rejection reason
        transfer.Reason = string.IsNullOrEmpty(transfer.Reason) 
            ? $"Rejected: {request.Reason}" 
            : $"{transfer.Reason} | Rejected: {request.Reason}";

        transfer.UpdatedAt = DateTime.UtcNow;
        
        var audit = new Assura.Domain.Entities.TransferApproval
        {
            TransferId = transfer.Id,
            ApprovedByUserId = request.DivisionHeadId,
            FromStatus = oldStatus,
            ToStatus = TransferStatus.RejectedByDivisionHead,
            Comments = $"Rejected by Division Head: {request.Reason}",
            ApprovedAt = DateTime.UtcNow
        };
        _context.TransferApprovals.Add(audit);

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
