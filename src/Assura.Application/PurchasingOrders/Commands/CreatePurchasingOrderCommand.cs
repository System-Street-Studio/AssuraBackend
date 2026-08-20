using Assura.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Assura.Domain.Entities;
using MediatR;
using FluentValidation;

namespace Assura.Application.PurchasingOrders.Commands;

public record CreatePurchasingOrderCommand : IRequest<int>
{
    public string SupplierName { get; init; } = string.Empty; // In a real app, you'd use SupplierId
    public List<CreatePurchasingOrderItemDto> Items { get; init; } = new();
    public int? RequestId { get; init; }
    public int? DivisionId { get; init; }
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

public class CreatePurchasingOrderItemDtoValidator : AbstractValidator<CreatePurchasingOrderItemDto>
{
    public CreatePurchasingOrderItemDtoValidator()
    {
        RuleFor(x => x.ItemName).NotEmpty().WithMessage("Item name is required.");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0.");
    }
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

        int? divisionId = request.DivisionId;
        AssetRequest? linkedAssetRequest = null;
        Request? linkedRequest = null;

        // The pending-requests queue (GetPendingAssetRequestsQuery) merges two tables with
        // unqualified ids: `Requests` rows keep their real positive id, `AssetRequests` rows
        // are given a negated id to avoid collisions. Un-negate before querying `AssetRequests`
        // so the originating request actually gets marked Approved and drops out of the queue.
        if (request.RequestId.HasValue)
        {
            if (request.RequestId.Value < 0)
            {
                var actualId = Math.Abs(request.RequestId.Value);
                var assetReq = await _context.AssetRequests.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == actualId && r.Status == Assura.Domain.Enums.RequestStatus.PendingProcurement, cancellationToken);
                if (assetReq != null)
                {
                    assetReq.Status = Assura.Domain.Enums.RequestStatus.Approved;
                    if (!divisionId.HasValue)
                    {
                        divisionId = assetReq.DivisionId ?? assetReq.User?.DivisionId;
                    }
                    linkedAssetRequest = assetReq;
                }
            }
            else
            {
                var req = await _context.Requests.Include(r => r.Requester).FirstOrDefaultAsync(r => r.Id == request.RequestId.Value && r.Status == Assura.Domain.Constants.RequestWorkflowStatus.PendingProcurement, cancellationToken);
                if (req != null)
                {
                    req.Status = Assura.Domain.Constants.RequestWorkflowStatus.Approved;
                    if (!divisionId.HasValue && req.Requester != null)
                    {
                        divisionId = req.Requester.DivisionId;
                    }
                    linkedRequest = req;
                }
            }
        }

        // 2. Create the Purchasing Order
        var po = new PurchasingOrder
        {
            OrderNumber = $"PO-{DateTime.UtcNow:yyyyMMddHHmmss}",
            OrderDate = DateTime.UtcNow,
            SupplierId = supplier.Id,
            DivisionId = divisionId,
            Status = "Pending"
        };

        // Record which request (if any) this PO is meant to satisfy, so that once an asset is
        // registered against it and the order is marked complete, UpdatePurchasingOrderStatusCommand
        // can find its way back and finish the handover instead of the request being stuck at
        // "Approved" forever with no asset ever assigned to the original requester.
        if (linkedAssetRequest != null) linkedAssetRequest.PurchasingOrder = po;
        if (linkedRequest != null) linkedRequest.PurchasingOrder = po;
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
