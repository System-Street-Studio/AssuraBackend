using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assura.Application.PurchasingOrders.Queries;

public record GetPurchasingOrdersQuery(bool UnregisteredOnly = false) : IRequest<List<PurchasingOrderSummaryDto>>;

public class GetPurchasingOrdersQueryHandler : IRequestHandler<GetPurchasingOrdersQuery, List<PurchasingOrderSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPurchasingOrdersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PurchasingOrderSummaryDto>> Handle(GetPurchasingOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PurchasingOrders
            .Include(x => x.Supplier)
            .Include(x => x.Division)
            .AsQueryable();

        if (request.UnregisteredOnly)
        {
            var registeredAssetNotes = await _context.Assets
                .Where(a => !a.IsDeleted && a.Notes != null)
                .Select(a => a.Notes!)
                .ToListAsync(cancellationToken);

            var pos = await query
                .OrderByDescending(x => x.OrderDate)
                .Select(x => new PurchasingOrderSummaryDto
                {
                    Id = x.Id,
                    OrderNumber = x.OrderNumber,
                    IssuedDate = x.OrderDate,
                    SupplierName = x.Supplier.Name,
                    DivisionId = x.DivisionId,
                    DivisionName = x.Division != null ? x.Division.Name : null,
                    Status = x.Status
                })
                .ToListAsync(cancellationToken);

            return pos.Where(p =>
                p.Status != "Completed" &&
                p.Status != "Registered" &&
                !registeredAssetNotes.Any(note => !string.IsNullOrEmpty(p.OrderNumber) && note.Contains(p.OrderNumber))
            ).ToList();
        }

        return await query
            .OrderByDescending(x => x.OrderDate)
            .Select(x => new PurchasingOrderSummaryDto
            {
                Id = x.Id,
                OrderNumber = x.OrderNumber,
                IssuedDate = x.OrderDate,
                SupplierName = x.Supplier.Name,
                DivisionId = x.DivisionId,
                DivisionName = x.Division != null ? x.Division.Name : null,
                Status = x.Status
            })
            .ToListAsync(cancellationToken);
    }
}

