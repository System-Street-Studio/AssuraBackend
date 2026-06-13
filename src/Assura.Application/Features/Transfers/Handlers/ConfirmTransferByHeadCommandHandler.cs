using MediatR;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;

namespace Assura.Application.Features.Transfers.Handlers;

public class ConfirmTransferByHeadCommandHandler : IRequestHandler<Commands.ConfirmTransferByHeadCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ConfirmTransferByHeadCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(Commands.ConfirmTransferByHeadCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _context.Transfers.FindAsync(new object[] { request.TransferId }, cancellationToken);

        if (transfer == null)
        {
            throw new Exception($"Transfer {request.TransferId} not found");
        }

        if (transfer.Status != TransferStatus.WaitingForFinalConfirmation)
        {
            throw new Exception($"Cannot confirm transfer in status {transfer.Status}");
        }

        // Update status to Active or ReadyForHandover
        // According to flow, after confirmation it's either ReadyForHandover or Active.
        transfer.Status = TransferStatus.Active;
        transfer.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
