using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.GRNs.Queries;

public record GetGRNsQuery : IRequest<List<GRNDto>>;

public class GetGRNsQueryHandler : IRequestHandler<GetGRNsQuery, List<GRNDto>>
{
    private readonly IApplicationDbContext _context;

    public GetGRNsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<GRNDto>> Handle(GetGRNsQuery request, CancellationToken cancellationToken)
    {
        return await _context.GRNs
            .AsNoTracking()
            .Include(g => g.PurchasingOrder)
                .ThenInclude(po => po.Supplier)
            .Include(g => g.Asset)
                .ThenInclude(a => a.Product)
            .OrderByDescending(g => g.ReceivedDate)
            .Select(g => new GRNDto
            {
                Id = g.Id,
                GrnNumber = g.GrnNumber,
                ReceivedDate = g.ReceivedDate,
                ReceivedBy = g.ReceivedBy,
                Notes = g.Notes,
                PurchasingOrderId = g.PurchasingOrderId,
                PurchasingOrderNumber = g.PurchasingOrder.OrderNumber,
                SupplierName = g.PurchasingOrder.Supplier.Name,
                AssetId = g.AssetId,
                AssetCode = g.Asset.AssetCode,
                ProductName = g.Asset.Product != null ? g.Asset.Product.Name : "-",
                CreatedAt = g.CreatedAt,
            })
            .ToListAsync(cancellationToken);
    }
}
