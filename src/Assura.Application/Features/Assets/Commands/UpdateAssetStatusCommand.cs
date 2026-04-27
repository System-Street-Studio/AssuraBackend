using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Assets.Commands;

public record UpdateAssetStatusCommand(int Id, AssetStatus Status) : IRequest<bool>;

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
