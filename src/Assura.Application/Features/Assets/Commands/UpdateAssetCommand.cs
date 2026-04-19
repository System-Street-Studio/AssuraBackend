using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Assets.Commands;

public record UpdateAssetCommand(AssetUpdateDto Asset) : IRequest<AssetDto?>;

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
                CategoryId = a.CategoryId,
                CategoryName = a.Category.Name,
                DivisionId = a.DivisionId,
                DivisionName = a.Division.Name,
                ProductId = a.ProductId,
                ProductName = a.Product.Name,
                SupplierId = a.SupplierId,
                SupplierName = a.Supplier.Name,
                AssignedUserId = a.AssignedUserId,
                AssignedUserName = a.AssignedUser != null ? $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}" : null
            })
            .FirstAsync(cancellationToken);

        return asset;
    }
}
