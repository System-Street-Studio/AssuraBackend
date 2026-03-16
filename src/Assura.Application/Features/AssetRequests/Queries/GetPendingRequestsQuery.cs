using MediatR;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.AssetRequests.Queries;

// 1. Query
public record GetPendingRequestsQuery : IRequest<List<AssetRequest>>;

// 2. Handler
public class GetPendingRequestsQueryHandler : IRequestHandler<GetPendingRequestsQuery, List<AssetRequest>>
{
    private readonly IApplicationDbContext _context;

    public GetPendingRequestsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<AssetRequest>> Handle(GetPendingRequestsQuery request, CancellationToken cancellationToken)
    {
        // Status 
        return await _context.AssetRequests
            .Where(x => x.Status == RequestStatus.Pending)
            .OrderByDescending(x => x.SubmittedDate)
            .ToListAsync(cancellationToken);
    }
}