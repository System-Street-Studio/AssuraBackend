using MediatR;
using Assura.Application.Features.Transfers.DTOs;
using Microsoft.EntityFrameworkCore;
using Assura.Application.Common.Interfaces;

namespace Assura.Application.Features.Transfers.Queries;

//  Query to get all transfers with optional filters for division, status, asset, and current holder
public class GetAllTransfersQuery : IRequest<List<TransferDto>>
{
 
    public int? DivisionId { get; set; }
    public string? Status { get; set; }
    public int AssetId { get; set; }
    public int? CurrentHolderId { get; set; }
    public int? DivisionHeadUserId { get; set; }
}

//  Handler for GetAllTransfersQuery to retrieve transfers based on the specified filters
public class GetAllTransfersQueryHandler
    : IRequestHandler<GetAllTransfersQuery, List<TransferDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllTransfersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TransferDto>> Handle(
        GetAllTransfersQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Transfers
            .Include(t => t.Asset)
                .ThenInclude(a => a.Product)
            .Include(t => t.FromDivision)
            .Include(t => t.ToDivision)
            .Include(t => t.TransferBy)
            .Include(t => t.TargetUser)
            .Include(t => t.CurrentHolder)
            .AsQueryable();

        //  FILTERS

        if (request.AssetId!= 0)
        {
            query = query.Where(t => t.AssetId == request.AssetId);
        }

        if (request.CurrentHolderId.HasValue)
        {
            query = query.Where(t => t.CurrentHolderId == request.CurrentHolderId);
        }

        if (request.DivisionId.HasValue)
        {
            query = query.Where(t =>
                t.FromDivisionId == request.DivisionId ||
                t.ToDivisionId == request.DivisionId);
        }

        if (request.DivisionHeadUserId.HasValue)
        {
            query = query.Where(t => 
                _context.Users.Any(u => 
                    u.Id == request.DivisionHeadUserId.Value && 
                    u.DivisionId == t.FromDivisionId && 
                    u.Role == Domain.Enums.UserRole.DivisionHead
                )
            );
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            if (Enum.TryParse<Domain.Enums.TransferStatus>(request.Status, out var statusEnum))
            {
                query = query.Where(t => t.Status == statusEnum);
            }
        }

        //  DTO Mapping
        var result = await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TransferDto
            {
                Id = t.Id,
                TransferNumber = t.TransferNumber,
                TransferDate = t.TransferDate,
                ReturnDate = t.ReturnDate,

                Reason = t.Reason,
                TransferPeriod = t.TransferPeriod,

                Status = t.Status.ToString(),

                //  Asset
                AssetId = t.AssetId,
                AssetTag = t.Asset.AssetTag,
                AssetCode = t.Asset.AssetCode,
                AssetStatus = t.Asset.Status.ToString(),
                ProductName =  t.Asset.Product.Name , 

                //  Request
                AssetRequestId = t.AssetRequestId,

                //  From Division
                FromDivisionId = t.FromDivisionId,
                FromDivisionName = t.FromDivision != null ? t.FromDivision.Name : null,

                //  To Division
                ToDivisionId = t.ToDivisionId,
                ToDivisionName = t.ToDivision != null ? t.ToDivision.Name : null,

                //  Users
                TransferById = t.TransferById,
                TransferByName = t.TransferBy != null ? t.TransferBy.Username : null,

                TargetUserId = t.TargetUserId ?? 0,
                TargetUserName = t.TargetUser != null ? t.TargetUser.Username : null,

                CurrentHolderId = t.CurrentHolderId,
                CurrentHolderName = t.CurrentHolder != null ? t.CurrentHolder.Username : null,

                //  Audit
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return result;
    }
}
