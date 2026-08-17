using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.GINs.Queries;

public record GetGINsQuery : IRequest<List<GINDto>>;

public class GetGINsQueryHandler : IRequestHandler<GetGINsQuery, List<GINDto>>
{
    private readonly IApplicationDbContext _context;

    public GetGINsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<GINDto>> Handle(GetGINsQuery request, CancellationToken cancellationToken)
    {
        return await _context.GINs
            .AsNoTracking()
            .Include(g => g.GRN)
            .Include(g => g.Asset)
                .ThenInclude(a => a.Product)
            .Include(g => g.Asset)
                .ThenInclude(a => a.AssignedUser)
            .OrderByDescending(g => g.AssignedDate)
            .Select(g => new GINDto
            {
                Id = g.Id,
                GinNumber = g.GinNumber,
                AssignedDate = g.AssignedDate,
                Condition = g.Condition,
                Notes = g.Notes,
                GRNId = g.GRNId,
                GrnNumber = g.GRN.GrnNumber,
                AssetId = g.AssetId,
                AssetCode = g.Asset.AssetCode,
                ProductName = g.Asset.Product != null ? g.Asset.Product.Name : "-",
                AssignedUserName = g.Asset.AssignedUser != null
                    ? $"{g.Asset.AssignedUser.FirstName} {g.Asset.AssignedUser.LastName}"
                    : null,
                CreatedAt = g.CreatedAt,
            })
            .ToListAsync(cancellationToken);
    }
}
