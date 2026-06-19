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
    
        var transfer = await _context.Transfers
            .Where(t => t.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (transfer == null)
            throw new Exception($"Transfer with ID {request.Id} not found");

        if (transfer.Status != TransferStatus.Active)
            throw new Exception($"Transfer cannot be returned from status {transfer.Status}.");

    
        var asset = await _context.Assets
            .Where(a => a.Id == transfer.AssetId)
            .Select(a => new Asset
            {
                Id = a.Id,
                Status = a.Status,
                UpdatedAt = a.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (asset != null)
        {
            _context.Assets.Attach(asset);
            asset.Status = AssetStatus.InUse;
            asset.UpdatedAt = DateTime.UtcNow;
        }

    
        transfer.Status = TransferStatus.Completed;
        transfer.UpdatedAt = DateTime.UtcNow;
        transfer.ReturnDate = DateTime.UtcNow;

    
        return await _context.SaveChangesAsync(cancellationToken) > 0;
    }
}