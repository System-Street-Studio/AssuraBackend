using MediatR;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.AssetRequests.Queries;

public record GetAllRequestsQuery(string? EmployeeId = null, bool IsDivisionHead = false) : IRequest<List<AssetRequest>>;

public class GetAllRequestsQueryHandler : IRequestHandler<GetAllRequestsQuery, List<AssetRequest>>
{
    private readonly IApplicationDbContext _context;

    public GetAllRequestsQueryHandler(IApplicationDbContext context) 
    {
        _context = context;
    }

    public async Task<List<AssetRequest>> Handle(GetAllRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AssetRequests.AsQueryable();

        //division head can see all requests from their division
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

        //regular employee can only see their own requests
        else if (!string.IsNullOrEmpty(request.EmployeeId))
        {
            query = query.Where(x => x.RequesterId == request.EmployeeId);
        }

        return await query
            .OrderByDescending(x => x.SubmittedDate)
            .ToListAsync(cancellationToken);
    }
}