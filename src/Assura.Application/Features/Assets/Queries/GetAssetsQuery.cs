using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Assets.Queries;

public record GetAssetsQuery(int? AssignedUserId = null) : IRequest<List<AssetDto>>;

public class GetAssetsQueryHandler : IRequestHandler<GetAssetsQuery, List<AssetDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAssetsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AssetDto>> Handle(GetAssetsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Assets
            .AsNoTracking()
            .Include(a => a.Product)
            .Include(a => a.Category)
            .Include(a => a.Division)
            .Include(a => a.Supplier)
            .Include(a => a.AssignedUser)
            .AsQueryable();

        if (request.AssignedUserId.HasValue)
        {
            query = query.Where(a => a.AssignedUserId == request.AssignedUserId.Value);
        }

        return await query
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
            .ToListAsync(cancellationToken);
    }
}
