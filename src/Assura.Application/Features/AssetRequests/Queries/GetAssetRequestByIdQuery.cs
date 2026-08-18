using MediatR;
using Assura.Application.Common.Interfaces;
using Assura.Application.Features.AssetRequests.DTOs;
using Microsoft.EntityFrameworkCore;
using Assura.Domain.Enums;

namespace Assura.Application.Features.AssetRequests.Queries;

public record GetAssetRequestByIdQuery : IRequest<AssetRequestDto?>
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public UserRole? Role { get; set; }
}

// Handler for retrieving a specific asset request by its ID, including related user, asset, and division information.
public class GetAssetRequestByIdQueryHandler : IRequestHandler<GetAssetRequestByIdQuery, AssetRequestDto?>
{
    private readonly IApplicationDbContext _context;

    public GetAssetRequestByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AssetRequestDto?> Handle(GetAssetRequestByIdQuery request, CancellationToken cancellationToken)
    {
        // AsNoTracking + a projection to AssetRequestDto (rather than returning the
        // tracked entity) avoids a JSON serialization cycle: AssetRequest.User ->
        // User.AssetRequests -> back to this same tracked instance, and separately
        // AssetAttachment.AssetRequest -> back to this instance. System.Text.Json has no
        // ReferenceHandler configured, so returning the raw entity throws on real data.
        var entity = await _context.AssetRequests
            .AsNoTracking()
            .Include(ar => ar.User)
            .Include(ar => ar.Division)
            .Include(ar => ar.Attachments)
            .FirstOrDefaultAsync(ar => ar.Id == request.Id, cancellationToken);

        if (entity == null) return null;

        // Roles with cross-user visibility over asset requests. Any other caller (e.g.
        // Employee) may only fetch a request they submitted themselves, to prevent IDOR.
        var isPrivileged = request.Role == UserRole.Admin
            || request.Role == UserRole.Procurement
            || request.Role == UserRole.Storekeeper
            || request.Role == UserRole.DivisionHead;

        // Division Head is privileged over the *whole org* by role, but must still be
        // scoped to their own division — unlike Admin/Procurement/Storekeeper, who
        // genuinely see everything. Matches the write-side check already enforced in
        // ApproveAssetRequestCommand/RejectAssetRequestCommand and GetRequestByIdQuery's
        // equivalent scoping for the sibling Requests entity — this query was the one
        // place that still let a Division Head read any other division's asset request.
        if (request.Role == UserRole.DivisionHead)
        {
            if (!request.UserId.HasValue) return null;

            var headDivisionId = await _context.Users
                .Where(u => u.Id == request.UserId.Value)
                .Select(u => u.DivisionId)
                .FirstOrDefaultAsync(cancellationToken);

            if (headDivisionId == null || entity.DivisionId != headDivisionId) return null;
        }
        else if (!isPrivileged && (!request.UserId.HasValue || entity.RequesterId != request.UserId.Value.ToString()))
        {
            return null;
        }

        return new AssetRequestDto
        {
            Id = entity.Id,
            RequesterId = entity.RequesterId,
            RequesterName = entity.RequesterName,
            AssetName = entity.AssetName,
            AssetCategory = entity.AssetCategory,
            Description = entity.Description ?? string.Empty,
            Reason = entity.Reason ?? string.Empty,
            Priority = entity.Priority,
            Status = entity.Status.ToString(),
            SubmittedDate = entity.SubmittedDate,
            Department = entity.Division?.Name ?? string.Empty,
            Email = entity.User?.Email ?? string.Empty,
            Quantity = entity.Quantity,
            RequestType = entity.RequestType,
            ProcessedByName = entity.ProcessedByName,
            ProcessorRemarks = entity.ProcessorRemarks,
            ProcessedAt = entity.ProcessedAt,
            Attachments = entity.Attachments.Select(a => new AttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                FileUrl = a.FileUrl,
                FileSize = a.FileSize,
                FileType = a.FileType,
                UploadedDate = a.UploadedDate
            }).ToList()
        };
    }
}
