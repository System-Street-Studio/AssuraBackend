using MediatR;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.AssetRequests.Queries;

public record GetAllRequestsQuery(string EmployeeId) : IRequest<List<AssetRequest>>;

public class GetAllRequestsQueryHandler : IRequestHandler<GetAllRequestsQuery, List<AssetRequest>>
{
    private readonly IApplicationDbContext _context;

    public GetAllRequestsQueryHandler(IApplicationDbContext context) 
    {
        _context = context;
    }

    public async Task<List<AssetRequest>> Handle(GetAllRequestsQuery request, CancellationToken cancellationToken)
    {
        // මෙතනදී EmployeeId එකට සමාන දත්ත විතරක් Filter කරනවා
        return await _context.AssetRequests
            .Where(x => x.RequesterId == request.EmployeeId) 
            .OrderByDescending(x => x.SubmittedDate)
            .ToListAsync(cancellationToken);
    }
}