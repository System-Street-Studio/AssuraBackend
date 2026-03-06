using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.PurchasingOrders.Queries;

public record GetPurchasingOrderByIdQuery(int Id) : IRequest<PurchasingOrderDto?>;

public class GetPurchasingOrderByIdQueryHandler : IRequestHandler<GetPurchasingOrderByIdQuery, PurchasingOrderDto?>
{
    private readonly IApplicationDbContext _context;

    public GetPurchasingOrderByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PurchasingOrderDto?> Handle(GetPurchasingOrderByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.PurchasingOrders
            .Include(x => x.Supplier)
            .Include(x => x.Items)
            .Where(x => x.Id == request.Id)
            .Select(x => new PurchasingOrderDto
            {
                Id = x.Id,
                OrderNumber = x.OrderNumber,
                OrderDate = x.OrderDate,
                TotalAmount = x.TotalAmount,
                Status = x.Status,
                SupplierName = x.Supplier.Name,
                Items = x.Items.Select(i => new PurchasingOrderItemDto
                {
                    Id = i.Id,
                    ItemName = i.ItemName,
                    Model = i.Model,
                    Warranty = i.Warranty,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Amount = i.Amount,
                    Discount = i.Discount,
                    DiscountedPrice = i.DiscountedPrice,
                    VatPercentage = i.VatPercentage,
                    VatAmount = i.VatAmount,
                    TotalPrice = i.TotalPrice,
                    SpecialNote = i.SpecialNote
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
