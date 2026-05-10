using MediatR;
using Microsoft.EntityFrameworkCore;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;

namespace Assura.Application.Features.Transfers.Commands;


public record ApproveTransferByHeadCommand(int TransferId) : IRequest<bool>;

public class ApproveTransferByHeadCommandHandler : IRequestHandler<ApproveTransferByHeadCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ApproveTransferByHeadCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

// Handle the command to approve a transfer by the division head
    public async Task<bool> Handle(ApproveTransferByHeadCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _context.Transfers
            .FirstOrDefaultAsync(t => t.Id == request.TransferId, cancellationToken);

        if (transfer == null)
            throw new Exception($"Transfer with ID {request.TransferId} not found");

        // Verify transfer is in the correct status
        if (transfer.Status != TransferStatus.PendingOwnerDivisionHeadApproval)
            throw new Exception($"Transfer cannot be approved from status {transfer.Status}. Expected status: {TransferStatus.PendingOwnerDivisionHeadApproval}");

        transfer.Status = TransferStatus.WaitingForFinalConfirmation;
        transfer.UpdatedAt = DateTime.UtcNow;

        var result = await _context.SaveChangesAsync(cancellationToken);
        return result > 0;
    }
}
