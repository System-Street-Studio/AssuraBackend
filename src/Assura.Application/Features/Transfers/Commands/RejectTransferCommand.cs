using MediatR;
using Microsoft.EntityFrameworkCore;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;

namespace Assura.Application.Features.Transfers.Commands;

// Command එක අර්ථ දැක්වීම
public record RejectTransferCommand(int TransferId) : IRequest<bool>;

public class RejectTransferCommandHandler : IRequestHandler<RejectTransferCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public RejectTransferCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(RejectTransferCommand request, CancellationToken cancellationToken)
    {
        
        var transfer = await _context.Transfers
            .FirstOrDefaultAsync(t => t.Id == request.TransferId, cancellationToken);

        if (transfer == null)
            throw new Exception("Transfer record not found");

        
        transfer.Status = TransferStatus.Rejected;
        transfer.UpdatedAt = DateTime.UtcNow;

       
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
