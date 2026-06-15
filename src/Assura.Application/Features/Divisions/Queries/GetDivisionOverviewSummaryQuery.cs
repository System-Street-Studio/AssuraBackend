using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assura.Application.Features.Divisions.Queries
{
    public class GetDivisionOverviewSummaryQuery : IRequest<DivisionOverviewSummaryDto>
    {
        public int DivisionId { get; set; }
        public GetDivisionOverviewSummaryQuery(int divisionId)
        {
            DivisionId = divisionId;
        }
    }

    public class GetDivisionOverviewSummaryQueryHandler : IRequestHandler<GetDivisionOverviewSummaryQuery, DivisionOverviewSummaryDto>
    {
        private readonly IApplicationDbContext _context;
        public GetDivisionOverviewSummaryQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

       public async Task<DivisionOverviewSummaryDto> Handle(GetDivisionOverviewSummaryQuery request, CancellationToken cancellationToken)
        {
            var assetsRaw = await _context.Assets
                .Where(a => a.DivisionId == request.DivisionId)
                .Select(a => new { a.PurchaseValue, a.Status })
                .ToListAsync(cancellationToken);

            var assetsCount = assetsRaw.Count();
            var assetsPurchaseValue = assetsRaw.Sum(x => x.PurchaseValue );
            var transferredAssetsCount = assetsRaw.Count(x => x.Status == AssetStatus.Transferred);

            var pendingRequestsCount = await _context.AssetRequests
                .Where(r => r.DivisionId == request.DivisionId && r.Status == RequestStatus.Pending)
                .CountAsync(cancellationToken);

            return new DivisionOverviewSummaryDto
            {
                AssetsCount = assetsCount,
                AssetsPurchaseValue = assetsPurchaseValue,
                PendingRequestsCount = pendingRequestsCount,
                TransferredAssetsCount = transferredAssetsCount
            };
        }
    }
}
