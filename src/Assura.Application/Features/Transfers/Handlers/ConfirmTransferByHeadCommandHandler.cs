using MediatR;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using Microsoft.EntityFrameworkCore;

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

        // At this stage the transfer is awaiting the *destination* (ToDivision) head's
        // final confirmation — see GetDivisionHeadTransferQueryHandler's "pending" tab,
        // which scopes the same status by ToDivisionId.
        var caller = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (caller?.DivisionId == null || caller.DivisionId != transfer.ToDivisionId)
        {
            throw new UnauthorizedAccessException("You may only confirm transfers destined for your own division.");
        }

        // Update status to Active or ReadyForHandover
        // According to flow, after confirmation it's either ReadyForHandover or Active.
        transfer.Status = TransferStatus.Active;
        transfer.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
