using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Assets.Commands;

public record CheckinAssetCommand(int Id, string Condition, string? Notes) : IRequest<AssetDto?>;

public class CheckinAssetCommandHandler : IRequestHandler<CheckinAssetCommand, AssetDto?>
{
    private readonly IApplicationDbContext _context;

    public CheckinAssetCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AssetDto?> Handle(CheckinAssetCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Assets
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (entity == null) return null;

        // Condition-based status update
        entity.Status = request.Condition == "Damaged" ? AssetStatus.UnderMaintenance : AssetStatus.InStore;
        entity.AssignedUserId = null;
        
        if (!string.IsNullOrEmpty(request.Notes))
        {
            entity.Notes = string.IsNullOrEmpty(entity.Notes) 
                ? $"Check-in: {request.Notes}" 
                : $"{entity.Notes} | Check-in: {request.Notes}";
        }

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
