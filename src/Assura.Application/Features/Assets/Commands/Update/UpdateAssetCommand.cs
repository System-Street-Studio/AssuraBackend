using MediatR;
using Assura.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Assets.Commands.Update;

public record UpdateAssetCommand(
    int Id,
    string Name,
    string Type,
    string SerialNumber,
    string Division,
    string Status
) : IRequest<bool>;

public class UpdateAssetCommandHandler : IRequestHandler<UpdateAssetCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateAssetCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateAssetCommand request, CancellationToken cancellationToken)
    {
        var asset = await _context.Assets.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
        if (asset is null) return false;

        // Update the mapped fields (matches what the frontend displays/edits)
        asset.AssetCode = request.Name;
        asset.SerialNumber = request.SerialNumber;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
