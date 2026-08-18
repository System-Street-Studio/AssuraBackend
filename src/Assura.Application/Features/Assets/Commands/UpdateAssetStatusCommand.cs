using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Assets.Commands;

/// <summary>
/// Command to update only the status of an existing asset.
/// Used for lightweight status transitions (e.g., InStore to UnderMaintenance).
/// </summary>
public record UpdateAssetStatusCommand(int Id, AssetStatus Status) : IRequest<bool>;

/// <summary>
/// Handler for executing the <see cref="UpdateAssetStatusCommand"/>.
/// Finds the asset, updates its status enum value, and saves changes.
/// </summary>
public class UpdateAssetStatusCommandHandler : IRequestHandler<UpdateAssetStatusCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateAssetStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateAssetStatusCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Assets
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (entity == null) return false;

        entity.Status = request.Status;
        
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
