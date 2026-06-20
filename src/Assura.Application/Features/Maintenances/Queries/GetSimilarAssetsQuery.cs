using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Maintenances.Queries;

public record GetSimilarAssetsQuery(int MaintenanceId) : IRequest<List<SimilarAssetDto>>;

public class GetSimilarAssetsQueryHandler : IRequestHandler<GetSimilarAssetsQuery, List<SimilarAssetDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSimilarAssetsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SimilarAssetDto>> Handle(GetSimilarAssetsQuery request, CancellationToken cancellationToken)
    {
        // Get the maintenance record to find asset category
        var maintenance = await _context.Maintenances
            .Include(m => m.Asset)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == request.MaintenanceId, cancellationToken);

        if (maintenance == null)
            return new List<SimilarAssetDto>();

        var categoryId = maintenance.Asset.CategoryId;

        // Find available assets in the same category that are InStore
        var similarAssets = await _context.Assets
            .Include(a => a.Product)
            .Include(a => a.Category)
            .AsNoTracking()
            .Where(a => a.CategoryId == categoryId
                     && a.Status == AssetStatus.InStore
                     && a.Id != maintenance.AssetId)
            .OrderBy(a => a.Product.Name)
            .Select(a => new SimilarAssetDto
            {
                Id = a.Id,
                AssetCode = a.AssetCode,
                ProductName = a.Product != null ? a.Product.Name : a.AssetCode,
                CategoryName = a.Category != null ? a.Category.Name : "",
                SerialNumber = a.SerialNumber,
                Status = a.Status.ToString(),
                PurchaseValue = a.PurchaseValue
            })
            .Take(20)
            .ToListAsync(cancellationToken);

        return similarAssets;
    }
}
