using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Assets.Queries;

/// <summary>
/// Query to retrieve a list of assets with role-based filtering.
/// - Storekeeper (no filters): returns all assets.
/// - DivisionHead (RequesterUserId set): returns only assets in their division.
/// - Employee (AssignedUserId set): returns only assets assigned to them.
/// </summary>
public record GetAssetsQuery(int? AssignedUserId = null, int? RequesterUserId = null, string? Role = null) : IRequest<List<AssetDto>>;

/// <summary>
/// Handler for <see cref="GetAssetsQuery"/>.
/// Builds a dynamic query based on the caller's role, eagerly loads all
/// navigation properties, and projects into a list of <see cref="AssetDto"/>.
/// </summary>
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

        // Division Head check: filter by their division

        if (request.Role == "DivisionHead" && request.RequesterUserId.HasValue)
        {
            var userDivisionId = await _context.Users
                .Where(u => u.Id == request.RequesterUserId.Value)
                .Select(u => u.DivisionId)
                .FirstOrDefaultAsync(cancellationToken);

            if (userDivisionId.HasValue)
            {
                query = query.Where(a => a.DivisionId == userDivisionId.Value);
            }
        }

        //employees get their own assets
        else if (request.AssignedUserId.HasValue)
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
            .ToListAsync(cancellationToken);
    }
}
