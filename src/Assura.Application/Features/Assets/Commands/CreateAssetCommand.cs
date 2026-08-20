using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using Assura.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace Assura.Application.Features.Assets.Commands;

/// <summary>
/// Command to create a new asset in the inventory.
/// Takes an <see cref="AssetCreateDto"/> containing all the initial asset details.
/// </summary>
public record CreateAssetCommand(AssetCreateDto Asset) : IRequest<AssetDto>;

/// <summary>
/// Handler for executing the <see cref="CreateAssetCommand"/>.
/// Maps the DTO to the Asset entity, generates a QR code, saves to the database,
/// and returns the fully populated DTO including navigation properties.
/// </summary>
public class CreateAssetCommandHandler : IRequestHandler<CreateAssetCommand, AssetDto>
{
    private readonly IApplicationDbContext _context;

    public CreateAssetCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AssetDto> Handle(CreateAssetCommand request, CancellationToken cancellationToken)
    {
        var code = string.IsNullOrWhiteSpace(request.Asset.AssetCode)
            ? $"AST-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}"
            : request.Asset.AssetCode.Trim();

        var entity = new Asset
        {
            AssetCode = code,
            AssetTag = request.Asset.AssetTag,
            AssetDate = request.Asset.AssetDate,
            Status = request.Asset.Status,
            SerialNumber = request.Asset.SerialNumber,
            PurchaseValue = request.Asset.PurchaseValue,
            Warranty = request.Asset.Warranty,
            Notes = request.Asset.Notes,
            CategoryId = request.Asset.CategoryId,
            DivisionId = request.Asset.DivisionId,
            ProductId = request.Asset.ProductId,
            SupplierId = request.Asset.SupplierId,
            AssignedUserId = request.Asset.AssignedUserId,
            PurchasingOrderId = request.Asset.PurchasingOrderId
        };

        // Generate QR Code
        using (var qrGenerator = new QRCodeGenerator())
        using (var qrCodeData = qrGenerator.CreateQrCode(entity.AssetCode, QRCodeGenerator.ECCLevel.Q))
        using (var qrCode = new PngByteQRCode(qrCodeData))
        {
            byte[] qrCodeAsBitmapByteArr = qrCode.GetGraphic(20);
            entity.QrCode = Convert.ToBase64String(qrCodeAsBitmapByteArr);
        }

        _context.Assets.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        // Auto-link AssetInforming record if this asset was registered from an arrival
        if (request.Asset.InformingId.HasValue && request.Asset.InformingId.Value > 0)
        {
            var informing = await _context.AssetInformings
                .FirstOrDefaultAsync(ai => ai.Id == request.Asset.InformingId.Value, cancellationToken);
            if (informing != null)
            {
                informing.Status = "GRN Recorded";
                informing.AssetId = entity.Id;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        else
        {
            // Fallback match by product name or item name for confirmed arrivals without direct link
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.Asset.ProductId, cancellationToken);
            var prodName = product?.Name?.Trim().ToLower() ?? "";
            if (!string.IsNullOrEmpty(prodName))
            {
                var informing = await _context.AssetInformings
                    .FirstOrDefaultAsync(ai => ai.Status == "Confirmed" && ai.AssetId == null &&
                        (ai.ItemName.Trim().ToLower() == prodName ||
                         prodName.Contains(ai.ItemName.Trim().ToLower()) ||
                         ai.ItemName.Trim().ToLower().Contains(prodName)), cancellationToken);
                if (informing != null)
                {
                    informing.Status = "GRN Recorded";
                    informing.AssetId = entity.Id;
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
        }

        // Fetch back with navigation properties
        var asset = await _context.Assets
            .AsNoTracking()
            .Include(a => a.Product)
            .Include(a => a.Category)
            .Include(a => a.Division)
            .Include(a => a.Supplier)
            .Include(a => a.AssignedUser)
            .Where(a => a.Id == entity.Id)
            .Select(a => new AssetDto
            {
                Id = a.Id,
                AssetCode = a.AssetCode,
                AssetTag = a.AssetTag,
                AssetDate = a.AssetDate,
                Status = a.Status,
                SerialNumber = a.SerialNumber,
                PurchaseValue = a.PurchaseValue,
                Warranty = a.Warranty,
                Notes = a.Notes,
                QrCode = a.QrCode,
                CategoryId = a.CategoryId ?? 0,
                CategoryName = a.Category != null ? a.Category.Name : "N/A",
                DivisionId = a.DivisionId ?? 0,
                DivisionName = a.Division != null ? a.Division.Name : "N/A",
                ProductId = a.ProductId ?? 0,
                ProductName = a.Product != null ? a.Product.Name : "N/A",
                SupplierId = a.SupplierId ?? 0,
                SupplierName = a.Supplier != null ? a.Supplier.Name : "N/A",
                AssignedUserId = a.AssignedUserId,
                AssignedUserName = a.AssignedUser != null ? $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}" : null
            })
            .FirstAsync(cancellationToken);

        return asset;
    }
}
