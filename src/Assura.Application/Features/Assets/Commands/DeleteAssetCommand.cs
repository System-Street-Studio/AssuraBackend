using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Assets.Commands;

/// <summary>
/// Command to delete an asset from the inventory by its ID.
/// </summary>
public record DeleteAssetCommand(int Id) : IRequest<bool>;

/// <summary>
/// Handler for executing the <see cref="DeleteAssetCommand"/>.
/// Finds the asset and removes it from the database, returning true if successful.
/// </summary>
public class DeleteAssetCommandHandler : IRequestHandler<DeleteAssetCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteAssetCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteAssetCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Assets
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (entity == null) return false;

        _context.Assets.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
