using Assura.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Assura.Domain.Entities;
using MediatR;

namespace Assura.Application.PurchasingOrders.Commands;

public record CreatePurchasingOrderCommand : IRequest<int>
{
    public string SupplierName { get; init; } = string.Empty; // In a real app, you'd use SupplierId
    public List<CreatePurchasingOrderItemDto> Items { get; init; } = new();
}

public record CreatePurchasingOrderItemDto
{
    public string ItemName { get; init; } = string.Empty;
    public string? Model { get; init; }
    public string? Warranty { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal Discount { get; init; }
    public decimal VatPercentage { get; init; }
    public string? SpecialNote { get; init; }
}

public class CreatePurchasingOrderCommandHandler : IRequestHandler<CreatePurchasingOrderCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreatePurchasingOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreatePurchasingOrderCommand request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[DEBUG] CreatePurchasingOrderCommandHandler: Starting for Supplier: '{request.SupplierName}'");

        // 1. Find or create supplier (simplified for this task)
        var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Name == request.SupplierName, cancellationToken);
        if (supplier == null)
        {
            Console.WriteLine($"[DEBUG] CreatePurchasingOrderCommandHandler: Supplier not found. Creating new supplier: '{request.SupplierName}'");
            supplier = new Supplier { Name = request.SupplierName };
            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync(cancellationToken);
        } else {
            Console.WriteLine($"[DEBUG] CreatePurchasingOrderCommandHandler: Supplier found with ID {supplier.Id}");
        }

        // 2. Create the Purchasing Order
        var po = new PurchasingOrder
        {
            OrderNumber = $"PO-{DateTime.UtcNow:yyyyMMddHHmmss}",
            OrderDate = DateTime.UtcNow,
            SupplierId = supplier.Id,
            Status = "Pending"
        };
        // Explicitly ensuring list is initialized if not already (check Domain entity)
        po.Items ??= new List<PurchasingOrderItem>();

        Console.WriteLine($"[DEBUG] CreatePurchasingOrderCommandHandler: Processing {request.Items?.Count} items");
        // 3. Create Items and calculate totals
        decimal totalAmount = 0;
        foreach (var itemDto in request.Items ?? new())
        {
            var amount = itemDto.Quantity * itemDto.UnitPrice;
            var discountAmount = amount * (itemDto.Discount / 100);
            var discountedPrice = amount - discountAmount;
            var vatAmount = discountedPrice * (itemDto.VatPercentage / 100);
            var totalPrice = discountedPrice + vatAmount;

            Console.WriteLine($"[DEBUG] CreatePurchasingOrderCommandHandler: Processing Item: '{itemDto.ItemName}', Total: {totalPrice}");
            
            var item = new PurchasingOrderItem
            {
                ItemName = itemDto.ItemName,
                Model = itemDto.Model,
                Warranty = itemDto.Warranty,
                Quantity = itemDto.Quantity,
                UnitPrice = itemDto.UnitPrice,
                Amount = amount,
                Discount = itemDto.Discount, // This is now stored as percentage
                DiscountedPrice = discountedPrice,
                VatPercentage = itemDto.VatPercentage,
                VatAmount = vatAmount,
                TotalPrice = totalPrice,
                SpecialNote = itemDto.SpecialNote
            };

            po.Items.Add(item);
            totalAmount += totalPrice;
        }

        po.TotalAmount = totalAmount;

        _context.PurchasingOrders.Add(po);
        Console.WriteLine("[DEBUG] CreatePurchasingOrderCommandHandler: Saving changes to database...");
        await _context.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"[DEBUG] CreatePurchasingOrderCommandHandler: Success! PO saved with Total: {totalAmount}");

        return po.Id;
    }
}
