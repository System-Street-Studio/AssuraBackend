using MediatR;
using Microsoft.EntityFrameworkCore;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;

namespace Assura.Application.Features.Transfers.Commands;


public record CancelTransferByHeadCommand(int TransferId) : IRequest<bool>;

public class CancelTransferByHeadCommandHandler : IRequestHandler<CancelTransferByHeadCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public CancelTransferByHeadCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

// Handle the command to cancel a transfer by the division head
    public async Task<bool> Handle(CancelTransferByHeadCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _context.Transfers
            .FirstOrDefaultAsync(t => t.Id == request.TransferId, cancellationToken);

        if (transfer == null)
            throw new Exception($"Transfer with ID {request.TransferId} not found");

        // Verify transfer is in the correct status
        if (transfer.Status != TransferStatus.PendingOwnerApproval)
            throw new Exception($"Transfer cannot be cancelled from status {transfer.Status}. Expected status: {TransferStatus.PendingOwnerApproval}");

        transfer.Status = TransferStatus.Cancelled;
        transfer.UpdatedAt = DateTime.UtcNow;

        var result = await _context.SaveChangesAsync(cancellationToken);
        return result > 0;
    }
}
