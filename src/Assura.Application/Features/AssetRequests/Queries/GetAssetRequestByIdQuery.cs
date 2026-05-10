using MediatR;
using Assura.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Assura.Domain.Entities;

namespace Assura.Application.Features.AssetRequests.Queries;

public record GetAssetRequestByIdQuery : IRequest<AssetRequest?>
{
    public int Id { get; set; }
}

// Handler for retrieving a specific asset request by its ID, including related user, asset, and division information.
public class GetAssetRequestByIdQueryHandler : IRequestHandler<GetAssetRequestByIdQuery, AssetRequest?>
{
    private readonly IApplicationDbContext _context;

    public GetAssetRequestByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AssetRequest?> Handle(GetAssetRequestByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.AssetRequests
            .Include(ar => ar.User)
            .Include(ar => ar.Asset)
            .Include(ar => ar.Division)
            .FirstOrDefaultAsync(ar => ar.Id == request.Id, cancellationToken);
    }
}
