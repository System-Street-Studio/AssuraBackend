using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.NewArrivals.Queries;

public record GetAssetInformingByIdQuery(int Id) : IRequest<AssetInformingDto?>;

public class GetAssetInformingByIdQueryHandler : IRequestHandler<GetAssetInformingByIdQuery, AssetInformingDto?>
{
    private readonly IApplicationDbContext _context;

    public GetAssetInformingByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AssetInformingDto?> Handle(GetAssetInformingByIdQuery request, CancellationToken cancellationToken)
    {
        var x = await _context.AssetInformings
            .AsNoTracking()
            .Include(a => a.Division)
            .Include(a => a.TargetEmployee)
            .Include(a => a.Asset)
            .ThenInclude(a => a!.Product)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (x is null) return null;

        return new AssetInformingDto
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
            TargetEmployeeName = x.TargetEmployee != null
                ? (x.TargetEmployee.FirstName + " " + x.TargetEmployee.LastName).Trim()
                : null,
            Remarks = x.Remarks,
            CreatedAt = x.CreatedAt,
            AssetId = x.AssetId,
            AssetCode = x.Asset != null ? x.Asset.AssetCode : null,
            PurchasingOrderId = x.PurchasingOrderId,
        };
    }
}
