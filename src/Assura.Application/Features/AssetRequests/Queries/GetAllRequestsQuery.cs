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

        
        if (!request.IsDivisionHead)
        {
           
            query = query.Where(x => x.RequesterId == request.EmployeeId);
        }

        
        return await query
            .OrderByDescending(x => x.SubmittedDate)
            .ToListAsync(cancellationToken);
    }
}