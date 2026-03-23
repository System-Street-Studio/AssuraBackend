using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using Assura.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Assets.Commands;

public record CreateAssetCommand(AssetCreateDto Asset) : IRequest<AssetDto>;

public class CreateAssetCommandHandler : IRequestHandler<CreateAssetCommand, AssetDto>
{
    private readonly IApplicationDbContext _context;

    public CreateAssetCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AssetDto> Handle(CreateAssetCommand request, CancellationToken cancellationToken)
    {
        var entity = new Asset
        {
            AssetCode = request.Asset.AssetCode,
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
            AssignedUserId = request.Asset.AssignedUserId
        };

        _context.Assets.Add(entity);
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
