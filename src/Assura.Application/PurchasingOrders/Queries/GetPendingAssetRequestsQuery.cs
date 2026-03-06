using Assura.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace Assura.Application.PurchasingOrders.Queries;

public record GetPendingAssetRequestsQuery : IRequest<List<AssetRequestDto>>;

public class GetPendingAssetRequestsQueryHandler : IRequestHandler<GetPendingAssetRequestsQuery, List<AssetRequestDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPendingAssetRequestsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AssetRequestDto>> Handle(GetPendingAssetRequestsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Requests
            .Include(x => x.Requester)
            .Where(x => x.Status == "Pending")
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new AssetRequestDto
            {
                Id = x.Id,
                EmployeeName = $"{x.Requester.FirstName} {x.Requester.LastName}",
                DivisionName = "Procurement", // Placeholder
                Date = x.CreatedAt,
                Specifications = x.Specifications,
                SpecialNote = x.SpecialNote
            })
            .ToListAsync(cancellationToken);
    }
}
