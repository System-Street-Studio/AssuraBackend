using MediatR;
using Assura.Application.Features.AssetRequests.DTOs;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.AssetRequests.Queries;

public record GetApprovedTransfersQuery(int? headId = null) : IRequest<List<ApprovedTransferRequestDto>>;


public class GetApprovedTransfersQueryHandler : IRequestHandler<GetApprovedTransfersQuery, List<ApprovedTransferRequestDto>>
{
    private readonly IApplicationDbContext _context;

    public GetApprovedTransfersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    // Retrieves a list of approved asset transfer requests, optionally filtered by division.
    public async Task<List<ApprovedTransferRequestDto>> Handle(GetApprovedTransfersQuery request, CancellationToken cancellationToken)
    {
        int? filterDivisionId = null;

        if (request.headId.HasValue) 
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.headId.Value, cancellationToken);
            
            filterDivisionId = user?.DivisionId;
            
        }

        var query = _context.AssetRequests
            .AsNoTracking()
            .Include(ar => ar.User)
            .Include(ar => ar.Division)
            .Where(ar => ar.RequestType == "Transfer" && ar.Status == RequestStatus.Approved)
            .AsQueryable();

        if (filterDivisionId.HasValue)
        {
            query = query.Where(ar => ar.DivisionId == filterDivisionId.Value);
        }


        var results = await query
            .OrderByDescending(ar => ar.SubmittedDate)
            .ToListAsync(cancellationToken);

        return results.Select(ar => new ApprovedTransferRequestDto
        {
            Id = ar.Id,
            RequesterName = ar.RequesterName,
            RequesterId = ar.RequesterId, 
            AssetName = ar.AssetName,
            AssetCategory = ar.AssetCategory,
            Status = ar.Status.ToString(),
            RequestType = ar.RequestType,
            SubmittedDate = ar.SubmittedDate,
            ApprovedDate = ar.UpdatedAt, // ApprovedDate would be when it was approved
            Priority = ar.Priority,
            Quantity = ar.Quantity,
            Reason = ar.Reason ?? string.Empty,
            Description = ar.Description ?? string.Empty,
            DivisionId = ar.DivisionId,
            DivisionName = ar.Division?.Name ?? string.Empty,
            TargetUserId = ar.UserId,
            TargetUserName = ar.User != null ? $"{ar.User.FirstName} {ar.User.LastName}" : string.Empty
        }).ToList();
    }
}
