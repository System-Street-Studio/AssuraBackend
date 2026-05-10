using MediatR;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.AssetRequests.Queries;

// 1. Query
public record GetPendingRequestsQuery(string? EmployeeId = null, bool IsDivisionHead = false) : IRequest<List<AssetRequest>>;

// 2. Handler
public class GetPendingRequestsQueryHandler : IRequestHandler<GetPendingRequestsQuery, List<AssetRequest>>
{
    private readonly IApplicationDbContext _context;

    public GetPendingRequestsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<AssetRequest>> Handle(GetPendingRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AssetRequests
            .Where(x => x.Status == RequestStatus.Pending) // Only fetch pending requests
            .AsQueryable();

        // Filter by employee or division head
        if (request.IsDivisionHead && !string.IsNullOrEmpty(request.EmployeeId))
        {
            if (int.TryParse(request.EmployeeId, out var userId))
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
                
                if (user?.DivisionId != null)
                {
                    query = query.Where(x => x.DivisionId == user.DivisionId);
                }
            }
        }
        else if (!string.IsNullOrEmpty(request.EmployeeId))
        {
            query = query.Where(x => x.RequesterId == request.EmployeeId);
        }

        return await query
            .OrderByDescending(x => x.SubmittedDate)
            .ToListAsync(cancellationToken);
    }
}