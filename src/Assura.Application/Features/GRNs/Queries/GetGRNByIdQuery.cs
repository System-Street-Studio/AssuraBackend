using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.GRNs.Queries;

public record GetGRNByIdQuery(int Id) : IRequest<GRNDto?>;

public class GetGRNByIdQueryHandler : IRequestHandler<GetGRNByIdQuery, GRNDto?>
{
    private readonly IApplicationDbContext _context;

    public GetGRNByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GRNDto?> Handle(GetGRNByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.GRNs
            .AsNoTracking()
            .Include(g => g.PurchasingOrder)
                .ThenInclude(po => po.Supplier)
            .Include(g => g.Asset)
                .ThenInclude(a => a.Product)
            .Where(g => g.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);
    }
}
