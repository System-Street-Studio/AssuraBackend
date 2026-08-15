using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Requests.Queries;

public record GetRequestByIdQuery(int Id, int? UserId = null, UserRole? Role = null) : IRequest<RequestDto?>;

public class GetRequestByIdQueryHandler : IRequestHandler<GetRequestByIdQuery, RequestDto?>
{
    private readonly IApplicationDbContext _context;

    public GetRequestByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RequestDto?> Handle(GetRequestByIdQuery request, CancellationToken cancellationToken)
    {
        // Roles with cross-user visibility over requests. Every other caller (e.g. Employee)
        // may only fetch a request they submitted themselves, to prevent IDOR.
        var isPrivileged = request.Role == UserRole.Admin
            || request.Role == UserRole.Procurement
            || request.Role == UserRole.Storekeeper
            || request.Role == UserRole.DivisionHead;

        // Division Head is privileged over the *whole org* by role, but must still be
        // scoped to their own division — unlike Admin/Procurement/Storekeeper, who
        // genuinely see everything. Matches GetRequestsQueryHandler's list scoping.
        int? headDivisionId = null;
        if (request.Role == UserRole.DivisionHead && request.UserId.HasValue)
        {
            headDivisionId = await _context.Users
                .Where(u => u.Id == request.UserId.Value)
                .Select(u => u.DivisionId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        // Negative ID means this is an AssetRequest record
        if (request.Id < 0)
        {
            var actualId = Math.Abs(request.Id);
            var ar = await _context.AssetRequests
                .AsNoTracking()
                .Include(a => a.User)
                .Include(a => a.Division)
                .FirstOrDefaultAsync(a => a.Id == actualId, cancellationToken);

            if (ar == null) return null;

            if (request.Role == UserRole.DivisionHead)
            {
                if (headDivisionId == null || ar.DivisionId != headDivisionId) return null;
            }
            else if (!isPrivileged && (!request.UserId.HasValue || ar.UserId != request.UserId.Value))
            {
                return null;
            }

            return new RequestDto
            {
                Id = -ar.Id,
                RequesterId = ar.UserId ?? 0,
                RequestNumber = $"AR-{ar.Id}",
                Type = ar.RequestType ?? "Asset",
                Priority = ar.Priority ?? "Normal",
                Description = ar.Description ?? ar.Reason,
                Status = ar.Status.ToString(),
                CreatedAt = ar.SubmittedDate,
                RequesterName = ar.RequesterName ?? "N/A",
                Department = ar.Division?.Name ?? "N/A",
                AssetName = ar.AssetName
            };
        }

        var entity = await _context.Requests
            .AsNoTracking()
            .Include(r => r.Requester)
            .Include(r => r.Requester.Division)
            .Include(r => r.Asset)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (entity == null) return null;

        if (request.Role == UserRole.DivisionHead)
        {
            var inHeadDivision = headDivisionId.HasValue &&
                (entity.Requester.DivisionId == headDivisionId.Value ||
                 (entity.Type == RequestType.Transfer && entity.Asset != null && entity.Asset.DivisionId == headDivisionId.Value));

            if (!inHeadDivision) return null;
        }
        else if (!isPrivileged && (!request.UserId.HasValue || entity.RequesterId != request.UserId.Value))
        {
            return null;
        }

        return new RequestDto
        {
            Id = entity.Id,
            RequesterId = entity.RequesterId,
            RequestNumber = entity.RequestNumber,
            Type = entity.Type.ToString(),
            Priority = entity.Priority.ToString(),
            Description = entity.Description,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            RequesterName = $"{entity.Requester.FirstName} {entity.Requester.LastName}",
            Department = entity.Requester.Division != null ? entity.Requester.Division.Name : "N/A",
            AssetName = entity.Asset != null ? entity.Asset.AssetCode : null
        };
    }
}
