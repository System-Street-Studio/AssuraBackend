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

        var targetId = Math.Abs(request.Id);

        // 1. Check unified Requests table first
        var entity = await _context.Requests
            .AsNoTracking()
            .Include(r => r.Requester)
            .Include(r => r.Requester.Division)
            .Include(r => r.Division)
            .Include(r => r.Asset)
            .Include(r => r.Asset!.AssignedUser)
            .Include(r => r.Attachments)
            .FirstOrDefaultAsync(r => r.Id == targetId, cancellationToken);

        if (entity != null)
        {
            if (request.Role == UserRole.DivisionHead)
            {
                var reqDivisionId = entity.DivisionId ?? entity.Requester?.DivisionId;
                var inHeadDivision = headDivisionId.HasValue &&
                    (reqDivisionId == headDivisionId.Value ||
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
                Description = entity.Description ?? entity.Reason,
                Status = entity.Status,
                CreatedAt = entity.CreatedAt,
                RequesterName = entity.Requester != null ? $"{entity.Requester.FirstName} {entity.Requester.LastName}" : "N/A",
                Department = entity.Division != null ? entity.Division.Name : (entity.Requester?.Division != null ? entity.Requester.Division.Name : "N/A"),
                AssetName = entity.AssetName ?? entity.Asset?.AssetCode,
                AssetCode = entity.Asset?.AssetCode,
                AssetDivisionName = entity.Division != null ? entity.Division.Name : (entity.Asset != null && entity.Asset.Division != null ? entity.Asset.Division.Name : null),
                AssigneeName = entity.Asset != null && entity.Asset.AssignedUser != null
                    ? $"{entity.Asset.AssignedUser.FirstName} {entity.Asset.AssignedUser.LastName}"
                    : null
            };
        }

        // 2. Fallback to legacy AssetRequests if not found in Requests
        var ar = await _context.AssetRequests
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.Division)
            .Include(a => a.Asset!.AssignedUser)
            .FirstOrDefaultAsync(a => a.Id == targetId, cancellationToken);

        if (ar != null)
        {
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
                Id = ar.Id,
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
                AssetDivisionName = ar.Division?.Name,
                AssigneeName = ar.Asset != null && ar.Asset.AssignedUser != null
                    ? $"{ar.Asset.AssignedUser.FirstName} {ar.Asset.AssignedUser.LastName}"
                    : null
            };
        }

        return null;
    }
}
