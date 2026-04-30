using MediatR;
using Microsoft.EntityFrameworkCore;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;

namespace Assura.Application.Features.Transfers.Commands;

public record ApproveTransferCommand(int TransferId) : IRequest<bool>;

public class ApproveTransferCommandHandler : IRequestHandler<ApproveTransferCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ApproveTransferCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<bool> Handle(ApproveTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _context.Transfers.FindAsync(new object[] { request.TransferId }, cancellationToken: cancellationToken);
        if (transfer == null)
            throw new Exception($"Transfer with ID {request.TransferId} not found");
        
        transfer.Status = TransferStatus.WaitingForFinalConfirmation;
        transfer.UpdatedAt = DateTime.UtcNow;
        return await _context.SaveChangesAsync(cancellationToken) > 0;
    }

    // Confirm Command Handler (Pending -> Active status change)
}