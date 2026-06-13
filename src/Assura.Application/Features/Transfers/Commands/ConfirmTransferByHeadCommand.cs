using MediatR;
using Microsoft.EntityFrameworkCore;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;

namespace Assura.Application.Features.Transfers.Commands;

public record ConfirmTransferByHeadCommand(int TransferId) : IRequest<bool>;

public class ConfirmTransferByHeadCommandHandler : IRequestHandler<ConfirmTransferByHeadCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ConfirmTransferByHeadCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ConfirmTransferByHeadCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _context.Transfers
            .Include(t => t.Asset)
            .FirstOrDefaultAsync(t => t.Id == request.TransferId, cancellationToken);

        if (transfer == null)
            throw new Exception($"Transfer with ID {request.TransferId} not found");

        // Verify transfer is in the correct status
        if (transfer.Status != TransferStatus.WaitingForFinalConfirmation)
            throw new Exception($"Transfer cannot be confirmed from status {transfer.Status}. Expected status: {TransferStatus.WaitingForFinalConfirmation}");

        if (transfer.Asset == null)
            throw new Exception($"Transfer asset not found");

        // Update transfer status
        transfer.Status = TransferStatus.Active;
        transfer.UpdatedAt = DateTime.UtcNow;
        transfer.TransferDate =  DateTime.UtcNow;

        // Update asset status
        transfer.Asset.Status = AssetStatus.Transferred;
        transfer.Asset.UpdatedAt = DateTime.UtcNow;

        var result = await _context.SaveChangesAsync(cancellationToken);
        return result > 0;
    }
}
