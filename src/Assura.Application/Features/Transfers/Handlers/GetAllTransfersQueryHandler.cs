using MediatR;
using Microsoft.EntityFrameworkCore;
using Assura.Application.Features.Transfers.Queries;
using Assura.Domain.Entities;
using Assura.Application.Common.Interfaces;

namespace Assura.Application.Features.Transfers.Handlers;

public class GetAllTransfersQueryHandler : IRequestHandler<GetAllTransfersQuery, List<TransferDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllTransfersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TransferDto>> Handle(GetAllTransfersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Transfers
            .Include(t => t.Asset)
            .Include(t => t.AssetRequest)
            .Include(t => t.FromDivision)
            .Include(t => t.ToDivision)
            .Include(t => t.TransferBy)
            .Include(t => t.TargetUser)
            .Include(t => t.CurrentHolder)
            .AsQueryable();

        // Apply filters
        if (request.DivisionId.HasValue)
        {
            query = query.Where(t => t.FromDivisionId == request.DivisionId.Value || t.ToDivisionId == request.DivisionId.Value);
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            // Parse the status string to enum value
            if (int.TryParse(request.Status, out int statusValue))
            {
                query = query.Where(t => (int)t.Status == statusValue);
            }
            else if (Enum.TryParse<Domain.Enums.TransferStatus>(request.Status, out var statusEnum))
            {
                query = query.Where(t => t.Status == statusEnum);
            }
        }

        if (request.CurrentHolderId.HasValue)
        {
            query = query.Where(t => t.CurrentHolderId == request.CurrentHolderId.Value || t.TargetUserId == request.CurrentHolderId.Value);
        }

        if (request.AssetId.HasValue)
        {
            query = query.Where(t => t.AssetId == request.AssetId.Value);
        }

        // Note: Pagination removed as Page and PageSize properties are not available in GetAllTransfersQuery

        var transfers = await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TransferDto
            {
                Id = t.Id,
                TransferNumber = t.TransferNumber,
                TransferDate = t.TransferDate,
                ReturnDate = t.ReturnDate,
                Reason = t.Reason,
                Status = t.Status.ToString(),
                AssetRequestId = t.AssetRequestId,
                AssetId = t.AssetId,
                AssetTag = null, // Transfer entity doesn't have AssetTag property
                FromDivisionId = t.FromDivisionId,
                FromDivisionName = t.FromDivision.Name,
                ToDivisionId = t.ToDivisionId,
                ToDivisionName = t.ToDivision.Name,
                TransferById = t.TransferById,
                TransferByName = t.TransferBy.Username,
                TargetUserId = t.TargetUserId,
                TargetUserName = t.TargetUser.Username,
                CurrentHolderId = t.CurrentHolderId,
                CurrentHolderName = t.CurrentHolder != null ? t.CurrentHolder.Username : null,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt ?? DateTime.MinValue
            })
            .ToListAsync(cancellationToken);

        return transfers;
    }
}
