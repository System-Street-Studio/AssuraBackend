using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.PurchasingOrders.Queries;

public record GetPurchasingOrdersQuery : IRequest<List<PurchasingOrderSummaryDto>>;

public class GetPurchasingOrdersQueryHandler : IRequestHandler<GetPurchasingOrdersQuery, List<PurchasingOrderSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPurchasingOrdersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PurchasingOrderSummaryDto>> Handle(GetPurchasingOrdersQuery request, CancellationToken cancellationToken)
    {
        return await _context.PurchasingOrders
            .OrderByDescending(x => x.OrderDate)
            .Select(x => new PurchasingOrderSummaryDto
            {
                Id = x.Id,
                OrderNumber = x.OrderNumber,
                IssuedDate = x.OrderDate,
                DepartmentName = "Information Technology" // Placeholder until actual department/division logic is linked to PO
            })
            .ToListAsync(cancellationToken);
    }
}
