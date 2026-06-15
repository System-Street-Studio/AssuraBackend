using MediatR;
using Assura.Application.Common.Interfaces;
using Assura.Application.Features.AssetRequests.DTOs;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.AssetRequests.Queries;

// 1. Query
public record GetPendingRequestsQuery(string? EmployeeId = null, bool IsDivisionHead = false) : IRequest<List<AssetRequestDto>>;

// 2. Handler
public class GetPendingRequestsQueryHandler : IRequestHandler<GetPendingRequestsQuery, List<AssetRequestDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPendingRequestsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<AssetRequestDto>> Handle(GetPendingRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AssetRequests
            .Include(x => x.Asset)
            .Include(x => x.User)
            .Include(x => x.Division)
            .Include(x => x.Attachments)
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
                else
                {
                    query = query.Where(x => false);
                }
            }
        }
        else if (!string.IsNullOrEmpty(request.EmployeeId))
        {
            query = query.Where(x => x.RequesterId == request.EmployeeId);
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
            RequestType = x.RequestType,
            Attachments = x.Attachments.Select(a => new AttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                FileUrl = a.FileUrl,
                FileSize = a.FileSize,
                FileType = a.FileType,
                UploadedDate = a.UploadedDate
            }).ToList()
        }).ToList();
    }
}