using Assura.Application.Common.Interfaces;
using Assura.Application.Features.GRNs.Queries;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace Assura.Application.Features.GRNs.Commands;

/// <summary>
/// Command to record a Goods Received Note for an asset delivered against a
/// purchasing order. If AssetId is 0 or null, automatically creates/registers
/// the asset in the Asset Register with status InStore and links the GRN to it.
/// </summary>
public record CreateGRNCommand(
    int PurchasingOrderId,
    int? AssetId,
    DateTime ReceivedDate,
    string? ReceivedBy,
    string? Notes,
    int? InformingId = null,
    string? ItemName = null,
    string? Model = null
) : IRequest<GRNDto>;

public class CreateGRNCommandValidator : AbstractValidator<CreateGRNCommand>
{
    public CreateGRNCommandValidator()
    {
        RuleFor(x => x.PurchasingOrderId).GreaterThan(0);
        RuleFor(x => x.ReceivedDate)
            .Must(d => d <= DateTime.UtcNow.AddDays(1))
            .WithMessage("Received date cannot be in the future.");
        RuleFor(x => x.Notes).MaximumLength(1000);
        RuleFor(x => x.ReceivedBy).MaximumLength(200);
    }
}

public class CreateGRNCommandHandler : IRequestHandler<CreateGRNCommand, GRNDto>
{
    private readonly IApplicationDbContext _context;

    public CreateGRNCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GRNDto> Handle(CreateGRNCommand request, CancellationToken cancellationToken)
    {
        var purchasingOrder = await _context.PurchasingOrders
            .Include(po => po.Supplier)
            .Include(po => po.Items)
            .FirstOrDefaultAsync(po => po.Id == request.PurchasingOrderId, cancellationToken);
        if (purchasingOrder == null)
        {
            throw new ValidationException("Purchasing order not found.");
        }

        AssetInforming? informing = null;
        if (request.InformingId.HasValue && request.InformingId.Value > 0)
        {
            informing = await _context.AssetInformings
                .FirstOrDefaultAsync(ai => ai.Id == request.InformingId.Value, cancellationToken);
        }

        Asset? asset = null;

        if (request.AssetId.HasValue && request.AssetId.Value > 0)
        {
            asset = await _context.Assets
                .Include(a => a.Product)
                .FirstOrDefaultAsync(a => a.Id == request.AssetId.Value, cancellationToken);
            if (asset == null)
            {
                throw new ValidationException("Asset not found.");
            }

            var alreadyReceived = await _context.GRNs
                .AnyAsync(g => g.AssetId == request.AssetId.Value, cancellationToken);
            if (alreadyReceived)
            {
                throw new ValidationException("A GRN has already been recorded for this asset.");
            }
        }
        else
        {
            // Auto-register new Asset in the Asset Register
            var poItem = purchasingOrder.Items.FirstOrDefault();
            string itemName = !string.IsNullOrWhiteSpace(request.ItemName)
                ? request.ItemName.Trim()
                : (!string.IsNullOrWhiteSpace(informing?.ItemName) ? informing!.ItemName.Trim() : (poItem?.ItemName ?? "Purchased Asset"));

            string? model = !string.IsNullOrWhiteSpace(request.Model)
                ? request.Model.Trim()
                : (informing?.Model ?? poItem?.Model);

            string? warranty = poItem?.Warranty ?? informing?.Warranty;
            decimal purchaseValue = poItem != null && poItem.TotalPrice > 0
                ? poItem.TotalPrice
                : (informing != null && informing.PurchasedPrice > 0 ? informing.PurchasedPrice : purchasingOrder.TotalAmount);

            int? divisionId = purchasingOrder.DivisionId ?? informing?.DivisionId;

            // If informing wasn't found by ID, try matching by itemName & division
            if (informing == null)
            {
                informing = await _context.AssetInformings
                    .FirstOrDefaultAsync(ai => !ai.IsDeleted && ai.ItemName.ToLower() == itemName.ToLower() && ai.Status != "Completed", cancellationToken);
            }

            // 1. Resolve or create Product
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Name.ToLower() == itemName.ToLower(), cancellationToken);

            if (product == null)
            {
                product = new Product
                {
                    Name = itemName,
                    ModelNumber = model,
                    Description = $"Auto-created from PO {purchasingOrder.OrderNumber}"
                };
                _context.Products.Add(product);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // 2. Resolve Category based on item/product keywords
            int? categoryId = null;
            var text = $"{itemName} {model}".ToLower();
            var categories = await _context.Categories.Where(c => !c.IsDeleted).ToListAsync(cancellationToken);
            if (categories.Any())
            {
                if (text.Contains("chair") || text.Contains("table") || text.Contains("desk") || text.Contains("furniture") || text.Contains("cupboard"))
                {
                    categoryId = categories.FirstOrDefault(c => c.Name.ToLower().Contains("furniture"))?.Id;
                }
                else if (text.Contains("computer") || text.Contains("laptop") || text.Contains("printer") || text.Contains("screen") || text.Contains("monitor") || text.Contains("pc") || text.Contains("calculator"))
                {
                    categoryId = categories.FirstOrDefault(c => c.Name.ToLower().Contains("computer") || c.Name.ToLower().Contains("office"))?.Id;
                }
                else if (text.Contains("vehicle") || text.Contains("car") || text.Contains("van") || text.Contains("bike"))
                {
                    categoryId = categories.FirstOrDefault(c => c.Name.ToLower().Contains("vehicle"))?.Id;
                }

                categoryId ??= categories.First().Id;
            }

            // 3. Generate unique AssetCode
            string code = string.Empty;
            for (int attempt = 0; attempt < 10; attempt++)
            {
                var candidate = $"AST-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
                if (!await _context.Assets.AnyAsync(a => a.AssetCode == candidate, cancellationToken))
                {
                    code = candidate;
                    break;
                }
            }
            if (string.IsNullOrEmpty(code))
            {
                code = $"AST-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(10000, 99999)}";
            }

            // 4. Create Asset entity with InStore status
            asset = new Asset
            {
                AssetCode = code,
                AssetDate = request.ReceivedDate,
                Status = AssetStatus.InStore,
                PurchaseValue = purchaseValue,
                Warranty = warranty,
                Notes = $"Auto-registered via GRN against PO {purchasingOrder.OrderNumber}",
                CategoryId = categoryId,
                DivisionId = divisionId,
                ProductId = product.Id,
                SupplierId = purchasingOrder.SupplierId,
                AssignedUserId = null
            };

            // Generate QR Code
            try
            {
                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(asset.AssetCode, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrCodeAsBitmapByteArr = qrCode.GetGraphic(20);
                asset.QrCode = Convert.ToBase64String(qrCodeAsBitmapByteArr);
            }
            catch
            {
                // Non-fatal if QR code generation fails
            }

            _context.Assets.Add(asset);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // 5. Create GRN
        var grnNumber = $"GRN-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
        var grn = new GRN
        {
            GrnNumber = grnNumber,
            ReceivedDate = request.ReceivedDate,
            ReceivedBy = string.IsNullOrWhiteSpace(request.ReceivedBy) ? "Storekeeper" : request.ReceivedBy.Trim(),
            Notes = request.Notes,
            PurchasingOrderId = request.PurchasingOrderId,
            AssetId = asset.Id,
        };
        _context.GRNs.Add(grn);

        // 6. Update Purchasing Order status to Registered if it was Pending
        if (purchasingOrder.Status != "Completed" && purchasingOrder.Status != "Registered")
        {
            purchasingOrder.Status = "Registered";
        }

        // 7. Close out AssetInforming record if found
        if (informing != null)
        {
            informing.Status = "Completed";
            informing.Remarks = string.IsNullOrWhiteSpace(informing.Remarks)
                ? $"GRN {grnNumber} recorded. Asset {asset.AssetCode} registered."
                : $"{informing.Remarks} | GRN {grnNumber} recorded.";
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new GRNDto
        {
            Id = grn.Id,
            GrnNumber = grn.GrnNumber,
            ReceivedDate = grn.ReceivedDate,
            ReceivedBy = grn.ReceivedBy,
            Notes = grn.Notes,
            PurchasingOrderId = purchasingOrder.Id,
            PurchasingOrderNumber = purchasingOrder.OrderNumber,
            SupplierName = purchasingOrder.Supplier?.Name 
                ?? (await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == purchasingOrder.SupplierId, cancellationToken))?.Name 
                ?? "-",
            AssetId = asset.Id,
            AssetCode = asset.AssetCode,
            ProductName = asset.Product?.Name ?? (request.ItemName ?? "Asset"),
            CreatedAt = grn.CreatedAt,
        };
    }
}
