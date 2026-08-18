using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Requests.Queries;

public record GetRequestsQuery(int? UserId = null, UserRole? Role = null) : IRequest<List<RequestDto>>;

public class RequestDto
{
    public int Id { get; set; }
    public int RequesterId { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string? AssetName { get; set; }
    public string? AssetCode { get; set; }
    public string? AssetDivisionName { get; set; }
    public string? AssigneeName { get; set; }
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
        // ── 1. Query the standard Requests table ──
        var query = _context.Requests
            .Include(r => r.Requester)
            .Include(r => r.Requester.Division)
            .Include(r => r.Asset)
            .Include(r => r.Asset!.Division)
            .AsQueryable();

        if (request.Role == UserRole.DivisionHead && request.UserId.HasValue)
        {
            var headDivisionId = await _context.Users
                .Where(u => u.Id == request.UserId.Value)
                .Select(u => u.DivisionId)
                .FirstOrDefaultAsync(cancellationToken);

            if (headDivisionId.HasValue)
            {
                query = query.Where(r => 
                    r.Requester.DivisionId == headDivisionId.Value || 
                    (r.Type == RequestType.Transfer && r.Asset != null && r.Asset.DivisionId == headDivisionId.Value));
            }
            else
            {
                query = query.Where(_ => false);
            }
        }
        else if (request.Role != UserRole.Admin && request.Role != UserRole.Procurement && request.Role != UserRole.Storekeeper && request.UserId.HasValue)
        {
            query = query.Where(r => r.RequesterId == request.UserId.Value);
        }

        var standardResults = await query
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new RequestDto
            {
                Id = r.Id,
                RequesterId = r.RequesterId,
                RequestNumber = r.RequestNumber,
                Type = r.Type.ToString(),
                Priority = r.Priority.ToString(),
                Description = r.Description,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                RequesterName = $"{r.Requester.FirstName} {r.Requester.LastName}",
                Department = r.Requester.Division != null ? r.Requester.Division.Name : "N/A",
                AssetName = r.Asset != null ? r.Asset.AssetCode : null,
                AssetCode = r.Asset != null ? r.Asset.AssetCode : null,
                AssetDivisionName = r.Asset != null && r.Asset.Division != null ? r.Asset.Division.Name : null
            })
            .ToListAsync(cancellationToken);

        // ── 2. Query the AssetRequests table and map with negative IDs ──
        var arQuery = _context.AssetRequests
            .Include(ar => ar.User)
            .Include(ar => ar.Division)
            .AsQueryable();

        if (request.Role == UserRole.DivisionHead && request.UserId.HasValue)
        {
            var headDivisionId = await _context.Users
                .Where(u => u.Id == request.UserId.Value)
                .Select(u => u.DivisionId)
                .FirstOrDefaultAsync(cancellationToken);

            if (headDivisionId.HasValue)
            {
                arQuery = arQuery.Where(ar => ar.DivisionId == headDivisionId.Value);
            }
            else
            {
                arQuery = arQuery.Where(_ => false);
            }
        }
        else if (request.Role != UserRole.Admin && request.Role != UserRole.Procurement && request.Role != UserRole.Storekeeper && request.UserId.HasValue)
        {
            arQuery = arQuery.Where(ar => ar.UserId == request.UserId.Value);
        }

        var assetRequestResults = await arQuery
            .OrderByDescending(ar => ar.SubmittedDate)
            .ToListAsync(cancellationToken);

        var mappedAssetRequests = assetRequestResults.Select(ar => new RequestDto
        {
            Id = -ar.Id,  // Negative ID to avoid collision with Requests table
            RequesterId = ar.UserId ?? 0,
            RequestNumber = $"AR-{ar.Id}",
            Type = ar.RequestType ?? "Asset",
            Priority = ar.Priority ?? "Normal",
            Description = ar.Description ?? ar.Reason,
            Status = ar.Status.ToString(),
            CreatedAt = ar.SubmittedDate,
            RequesterName = ar.RequesterName ?? "N/A",
            Department = ar.Division?.Name ?? "N/A",
            AssetName = ar.AssetName,
            AssetCode = null,
            AssetDivisionName = ar.Division?.Name
        }).ToList();

        // ── 3. Combine and sort by date ──
        var combined = standardResults.Concat(mappedAssetRequests)
            .OrderByDescending(r => r.CreatedAt)
            .ToList();

        return combined;
    }
}
