using Assura.Application.Common.Interfaces;
using Assura.Domain.Constants;
using Assura.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace Assura.Application.PurchasingOrders.Queries;

public record GetPendingAssetRequestsQuery : IRequest<List<AssetRequestDto>>;

public class GetPendingAssetRequestsQueryHandler : IRequestHandler<GetPendingAssetRequestsQuery, List<AssetRequestDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPendingAssetRequestsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AssetRequestDto>> Handle(GetPendingAssetRequestsQuery request, CancellationToken cancellationToken)
    {
        var requestsList = await _context.Requests
            .Include(x => x.Requester)
                .ThenInclude(u => u.Division)
            .Include(x => x.Asset)
            .Where(x => x.Status == RequestWorkflowStatus.PendingProcurement)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new AssetRequestDto
            {
                Id = x.Id,
                EmployeeName = $"{x.Requester.FirstName} {x.Requester.LastName}",
                DivisionName = x.Requester.Division != null ? x.Requester.Division.Name : "N/A",
                Date = x.CreatedAt,
                Specifications = x.Specifications,
                SpecialNote = x.SpecialNote,
                Type = x.Type.ToString(),
                Description = x.Description,
                AssetId = x.AssetId,
                AssetName = x.Asset != null ? x.Asset.AssetCode : "N/A"
            })
            .ToListAsync(cancellationToken);

        var assetRequestsList = await _context.AssetRequests
            .Include(x => x.User)
                .ThenInclude(u => u.Division)
            .Include(x => x.Division)
            .Where(x => x.Status == RequestStatus.PendingProcurement)
            .OrderByDescending(x => x.SubmittedDate)
            .Select(x => new AssetRequestDto
            {
                Id = x.Id,
                EmployeeName = x.RequesterName,
                DivisionName = x.Division != null ? x.Division.Name : (x.User != null && x.User.Division != null ? x.User.Division.Name : "N/A"),
                Date = x.SubmittedDate,
                Specifications = x.Description,
                SpecialNote = x.Reason,
                Type = x.RequestType,
                Description = x.Description,
                AssetId = x.AssetId,
                AssetName = x.AssetName
            })
            .ToListAsync(cancellationToken);

        return requestsList.Concat(assetRequestsList)
            .OrderByDescending(x => x.Date)
            .ToList();
    }
}
