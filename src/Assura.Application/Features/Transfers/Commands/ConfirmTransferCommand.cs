using MediatR;
using Microsoft.EntityFrameworkCore;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;

namespace Assura.Application.Features.Transfers.Commands;

public record ConfirmTransferCommand(int TransferId) : IRequest<bool>;

public class ConfirmTransferCommandHandler : IRequestHandler<ConfirmTransferCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ConfirmTransferCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ConfirmTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _context.Transfers.Include(t => t.Asset).FirstOrDefaultAsync(t => t.Id == request.TransferId, cancellationToken);
        
        if (transfer == null)
            throw new Exception($"Transfer with ID {request.TransferId} not found");
        
        if (transfer.Asset == null)
            throw new Exception("Transfer asset not found");
        
        transfer.Status = TransferStatus.Active;
        transfer.UpdatedAt = DateTime.UtcNow;
      
        transfer.Asset.Status = AssetStatus.Transferred;
        transfer.Asset.UpdatedAt = DateTime.UtcNow;
      
        return await _context.SaveChangesAsync(cancellationToken) > 0;
    }
}