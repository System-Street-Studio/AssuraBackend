using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.NewArrivals.Queries;

public record GetAssetInformingsQuery : IRequest<List<AssetInformingDto>>;

public class GetAssetInformingsQueryHandler : IRequestHandler<GetAssetInformingsQuery, List<AssetInformingDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAssetInformingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AssetInformingDto>> Handle(GetAssetInformingsQuery request, CancellationToken cancellationToken)
    {
        return await _context.AssetInformings
            .Include(x => x.Division)
            .Include(x => x.TargetEmployee)
            .Include(x => x.Asset)
            .ThenInclude(a => a!.Product)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new AssetInformingDto
            {
                Id = x.Id,
                ItemName = x.ItemName,
                Model = x.Model,
                Warranty = x.Warranty,
                Quantity = x.Quantity,
                PurchasedDate = x.PurchasedDate,
                PurchasedPrice = x.PurchasedPrice,
                Status = x.Status,
                DivisionId = x.DivisionId,
                DivisionName = x.Division != null ? x.Division.Name : string.Empty,
                TargetEmployeeId = x.TargetEmployeeId,
                TargetEmployeeName = x.TargetEmployee != null ? (x.TargetEmployee.FirstName + " " + x.TargetEmployee.LastName).Trim() : null,
                Remarks = x.Remarks,
                CreatedAt = x.CreatedAt,
                AssetId = x.AssetId,
                AssetCode = x.Asset != null ? x.Asset.AssetCode : null,
                PurchasingOrderId = x.PurchasingOrderId
            })
            .ToListAsync(cancellationToken);
    }
}
