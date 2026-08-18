using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.GINs.Queries;

public record GetGINByIdQuery(int Id) : IRequest<GINDto?>;

public class GetGINByIdQueryHandler : IRequestHandler<GetGINByIdQuery, GINDto?>
{
    private readonly IApplicationDbContext _context;

    public GetGINByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GINDto?> Handle(GetGINByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.GINs
            .AsNoTracking()
            .Include(g => g.GRN)
            .Include(g => g.Asset)
                .ThenInclude(a => a.Product)
            .Include(g => g.Asset)
                .ThenInclude(a => a.AssignedUser)
            .Where(g => g.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);
    }
}
