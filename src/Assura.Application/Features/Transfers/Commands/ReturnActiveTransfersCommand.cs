using MediatR;
using Microsoft.EntityFrameworkCore;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using Assura.Domain.Entities; 
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assura.Application.Features.Transfers.Commands;

public record ReturnActiveTransferCommand(int Id) : IRequest<bool>;

public class ReturnActiveTransferCommandHandler : IRequestHandler<ReturnActiveTransferCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ReturnActiveTransferCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ReturnActiveTransferCommand request, CancellationToken cancellationToken)
        {
            
            var transferToUpdate = await _context.Transfers
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

            if (transferToUpdate == null)
                throw new Exception($"Transfer with ID {request.Id} not found");

            if (transferToUpdate.Status != TransferStatus.Active)
                throw new Exception($"Transfer cannot be returned from status {transferToUpdate.Status}. Expected status: {TransferStatus.Active}");

            
            transferToUpdate.Status = TransferStatus.Completed;
            transferToUpdate.UpdatedAt = DateTime.UtcNow;
            transferToUpdate.ReturnDate = DateTime.UtcNow;
        

           
            var assetToUpdate = await _context.Assets
                .FirstOrDefaultAsync(a => a.Id == transferToUpdate.AssetId, cancellationToken);

            if (assetToUpdate != null)
            {
                assetToUpdate.Status = AssetStatus.InUse;
                assetToUpdate.UpdatedAt = DateTime.UtcNow;
            }

           
            var result = await _context.SaveChangesAsync(cancellationToken);
            return result > 0;
        }
}