using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Requests.Queries;

public record GetRequestsQuery(int? UserId = null, UserRole? Role = null) : IRequest<List<RequestDto>>;

public class RequestDto
{
    public int Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string? AssetName { get; set; }
}

public class GetRequestsQueryHandler : IRequestHandler<GetRequestsQuery, List<RequestDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRequestsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<RequestDto>> Handle(GetRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Requests
            .Include(r => r.Requester)
            .Include(r => r.Requester.Division)
            .Include(r => r.Asset)
            .AsQueryable();

        if (request.Role != UserRole.Admin && request.Role != UserRole.Procurement && request.Role != UserRole.Storekeeper && request.UserId.HasValue)
        {
            query = query.Where(r => r.RequesterId == request.UserId.Value);
        }

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new RequestDto
            {
                Id = r.Id,
                RequestNumber = r.RequestNumber,
                Type = r.Type.ToString(),
                Priority = r.Priority.ToString(),
                Description = r.Description,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                RequesterName = $"{r.Requester.FirstName} {r.Requester.LastName}",
                Department = r.Requester.Division != null ? r.Requester.Division.Name : "N/A",
                AssetName = r.Asset != null ? r.Asset.AssetCode : null
            })
            .ToListAsync(cancellationToken);
    }
}
