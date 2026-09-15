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
        var items = await _context.AssetInformings
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

        var unresolvedPoIds = items
            .Where(x => !x.TargetEmployeeId.HasValue && x.PurchasingOrderId.HasValue)
            .Select(x => x.PurchasingOrderId!.Value)
            .Distinct()
            .ToList();

        if (unresolvedPoIds.Count > 0)
        {
            var requests = await _context.Requests
                .Include(r => r.Requester)
                .Where(r => r.PurchasingOrderId.HasValue && unresolvedPoIds.Contains(r.PurchasingOrderId.Value))
                .ToListAsync(cancellationToken);

            var assetRequests = await _context.AssetRequests
                .Include(ar => ar.User)
                .Where(ar => ar.PurchasingOrderId.HasValue && unresolvedPoIds.Contains(ar.PurchasingOrderId.Value))
                .ToListAsync(cancellationToken);

            foreach (var item in items)
            {
                if (!item.TargetEmployeeId.HasValue && item.PurchasingOrderId.HasValue)
                {
                    var req = requests.FirstOrDefault(r => r.PurchasingOrderId == item.PurchasingOrderId.Value);
                    if (req?.Requester != null)
                    {
                        item.TargetEmployeeId = req.RequesterId;
                        item.TargetEmployeeName = $"{req.Requester.FirstName} {req.Requester.LastName}".Trim();
                    }
                    else
                    {
                        var ar = assetRequests.FirstOrDefault(a => a.PurchasingOrderId == item.PurchasingOrderId.Value);
                        if (ar?.User != null)
                        {
                            item.TargetEmployeeId = ar.UserId;
                            item.TargetEmployeeName = $"{ar.User.FirstName} {ar.User.LastName}".Trim();
                        }
                    }
                }
            }
        }

        return items;
    }
}
