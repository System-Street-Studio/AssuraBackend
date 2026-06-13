using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Divisions.Queries;

public record GetDivisionOverviewSummaryQuery(int DivisionId) : IRequest<DivisionOverviewSummaryDto>;

public class GetDivisionOverviewSummaryQueryHandler : IRequestHandler<GetDivisionOverviewSummaryQuery, DivisionOverviewSummaryDto>
{
    private readonly IApplicationDbContext _context;

    public GetDivisionOverviewSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DivisionOverviewSummaryDto> Handle(GetDivisionOverviewSummaryQuery request, CancellationToken cancellationToken)
    {
        var divId = request.DivisionId;

        // 1. Total Assets & 2. Total Purchase Value
        var assetsQuery = _context.Assets.Where(a => a.DivisionId == divId);
        var assetsCount = await assetsQuery.CountAsync(cancellationToken);
        var assetsPurchaseValue = await assetsQuery.SumAsync(a => (decimal?)a.PurchaseValue, cancellationToken) ?? 0;

        // 3. Pending Requests
        var pendingRequestsCount = await _context.AssetRequests
            .CountAsync(r => r.DivisionId == divId && r.Status == RequestStatus.Pending, cancellationToken);

        // 4. Transferred Assets
        // Assuming 'Active' transfers represent assets successfully handed over/transferred.
        var transferredAssetsCount = await _context.Transfers
            .CountAsync(t => t.FromDivisionId == divId && t.Status == TransferStatus.Active, cancellationToken);

        return new DivisionOverviewSummaryDto(
            assetsCount,
            assetsPurchaseValue,
            pendingRequestsCount,
            transferredAssetsCount
        );
    }
}
