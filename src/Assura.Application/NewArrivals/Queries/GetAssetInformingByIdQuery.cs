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

        var dto = new AssetInformingDto
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

        if (!dto.TargetEmployeeId.HasValue && dto.PurchasingOrderId.HasValue)
        {
            var req = await _context.Requests
                .Include(r => r.Requester)
                .FirstOrDefaultAsync(r => r.PurchasingOrderId == dto.PurchasingOrderId.Value, cancellationToken);
            if (req?.Requester != null)
            {
                dto.TargetEmployeeId = req.RequesterId;
                dto.TargetEmployeeName = $"{req.Requester.FirstName} {req.Requester.LastName}".Trim();
            }
            else
            {
                var ar = await _context.AssetRequests
                    .Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.PurchasingOrderId == dto.PurchasingOrderId.Value, cancellationToken);
                if (ar?.User != null)
                {
                    dto.TargetEmployeeId = ar.UserId;
                    dto.TargetEmployeeName = $"{ar.User.FirstName} {ar.User.LastName}".Trim();
                }
            }
        }

        return dto;
    }
}
