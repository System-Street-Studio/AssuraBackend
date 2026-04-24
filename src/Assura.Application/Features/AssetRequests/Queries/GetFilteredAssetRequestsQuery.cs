using MediatR;
using Assura.Application.Common.Interfaces;
using Assura.Application.Features.AssetRequests.DTOs;
using Assura.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.AssetRequests.Queries;

public record GetFilteredAssetRequestsQuery(string? Status = null, string? Type = null, string? EmployeeId = null, bool IsDivisionHead = false) : IRequest<List<AssetRequestDto>>;

public class GetFilteredAssetRequestsQueryHandler : IRequestHandler<GetFilteredAssetRequestsQuery, List<AssetRequestDto>>
{
    private readonly IApplicationDbContext _context;

    public GetFilteredAssetRequestsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AssetRequestDto>> Handle(GetFilteredAssetRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AssetRequests.Include(x => x.User).Include(x => x.Division).AsQueryable();

        // Filter by employee
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

        // Filter by status
        if (!string.IsNullOrEmpty(request.Status))
        {
            if (Enum.TryParse<RequestStatus>(request.Status, true, out var statusEnum))
            {
                query = query.Where(x => x.Status == statusEnum);
            }
        }

        // Filter by type
        if (!string.IsNullOrEmpty(request.Type))
        {
            query = query.Where(x => x.RequestType == request.Type);
        }

        var results = await query
            .OrderByDescending(x => x.SubmittedDate)
            .ToListAsync(cancellationToken);

        return results.Select(x => new AssetRequestDto
        {
            Id = x.Id,
            RequesterId = x.RequesterId,
            RequesterName = x.RequesterName,
            AssetName = x.AssetName,
            AssetCategory = x.AssetCategory,
            Description = x.Description ?? string.Empty,
            Reason = x.Reason ?? string.Empty,
            Priority = x.Priority,
            Status = x.Status.ToString(),
            SubmittedDate = x.SubmittedDate,
            Department = x.Division?.Name ?? string.Empty,
            Email = x.User?.Email ?? string.Empty,
            Quantity = x.Quantity,
            RequestType = x.RequestType
        }).ToList();
    }
}
