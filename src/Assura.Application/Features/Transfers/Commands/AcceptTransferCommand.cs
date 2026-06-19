using MediatR;
using Microsoft.EntityFrameworkCore;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;

namespace Assura.Application.Features.Transfers.Commands;


public record AcceptTransferCommand(int TransferId) : IRequest<bool>;

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

        
        transfer.Status = TransferStatus.PendingOwnerDivisionHeadApproval;
        transfer.UpdatedAt = DateTime.UtcNow;

       
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
