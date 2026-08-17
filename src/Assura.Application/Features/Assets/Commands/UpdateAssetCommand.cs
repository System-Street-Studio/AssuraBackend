using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Assets.Commands;

/// <summary>
/// Command to update the details of an existing asset.
/// Takes an <see cref="AssetUpdateDto"/> containing the updated details.
/// </summary>
public record UpdateAssetCommand(AssetUpdateDto Asset) : IRequest<AssetDto?>;

/// <summary>
/// Handler for executing the <see cref="UpdateAssetCommand"/>.
/// Finds the asset, applies all provided updates, saves to the database,
/// and returns the fully populated DTO including navigation properties.
/// </summary>
public class UpdateAssetCommandHandler : IRequestHandler<UpdateAssetCommand, AssetDto?>
{
    private readonly IApplicationDbContext _context;

    public UpdateAssetCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AssetDto?> Handle(UpdateAssetCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Assets
            .FirstOrDefaultAsync(a => a.Id == request.Asset.Id, cancellationToken);

        if (entity == null) return null;

        entity.AssetCode = request.Asset.AssetCode;
        entity.AssetTag = request.Asset.AssetTag;
        entity.AssetDate = request.Asset.AssetDate;
        entity.Status = request.Asset.Status;
        entity.SerialNumber = request.Asset.SerialNumber;
        entity.PurchaseValue = request.Asset.PurchaseValue;
        entity.Warranty = request.Asset.Warranty;
        entity.Notes = request.Asset.Notes;
        entity.CategoryId = request.Asset.CategoryId;
        entity.DivisionId = request.Asset.DivisionId;
        entity.ProductId = request.Asset.ProductId;
        entity.SupplierId = request.Asset.SupplierId;
        entity.AssignedUserId = request.Asset.AssignedUserId;

        await _context.SaveChangesAsync(cancellationToken);

        // Fetch back with navigation properties
        var asset = await _context.Assets
            .AsNoTracking()
            .Include(a => a.Product)
            .Include(a => a.Category)
            .Include(a => a.Division)
            .Include(a => a.Supplier)
            .Include(a => a.AssignedUser)
            .Include(a => a.LastVerifiedByUser)
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
                LastVerifiedAt = a.LastVerifiedAt,
                LastVerifiedByName = a.LastVerifiedByUser != null ? $"{a.LastVerifiedByUser.FirstName} {a.LastVerifiedByUser.LastName}" : null,
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
