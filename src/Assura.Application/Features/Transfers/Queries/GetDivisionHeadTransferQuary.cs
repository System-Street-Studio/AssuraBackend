using MediatR;
using Assura.Application.Features.Transfers.DTOs;
using Microsoft.EntityFrameworkCore;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using Assura.Application.DTOs;


namespace Assura.Application.Features.Transfers.Queries;

public record GetDivisionHeadTransferQuery(string Tab, int LoginUserId) : IRequest<List<TransferDto>>;

public class GetDivisionHeadTransferQueryHandler : IRequestHandler<GetDivisionHeadTransferQuery, List<TransferDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDivisionHeadTransferQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TransferDto>> Handle(GetDivisionHeadTransferQuery request, CancellationToken cancellationToken)
    {
        var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.LoginUserId);
        
        if (currentUser == null)
            return new List<TransferDto>();
        
        var headDivisionId = currentUser.DivisionId;

        var query = _context.Transfers
            .Include(t => t.Asset).ThenInclude(a => a.Product)
            .Include(t => t.TargetUser)
            .Include(t => t.CurrentHolder)
            .Include(t => t.TransferBy)
            .Include(t => t.ToDivision)
            .Include(t => t.FromDivision)
            .AsNoTracking();

       // Apply filters based on the selected tab and the division head's division
        query = request.Tab.ToLower() switch
        {
            "outgoing" => query.Where(t => t.FromDivisionId == headDivisionId 
                                        && t.Status == TransferStatus.PendingOwnerApproval),

            "incoming" => query.Where(t => t.FromDivisionId == headDivisionId 
                                        && t.Status == TransferStatus.PendingOwnerDivisionHeadApproval),

            "pending" => query.Where(t => t.ToDivisionId == headDivisionId 
                                        && t.Status == TransferStatus.WaitingForFinalConfirmation),

            "active" => query.Where(t => t.Status == TransferStatus.Active 
                                        && (t.FromDivisionId == headDivisionId || t.ToDivisionId == headDivisionId)),

            "completed" => query.Where(t => t.Status == TransferStatus.Completed 
                                           && (t.FromDivisionId == headDivisionId || t.ToDivisionId == headDivisionId)),

            _ => query
        };

        return await query.Select(t => new TransferDto
        {
            Id = t.Id,
            AssetTag = t.Asset != null ? t.Asset.AssetTag : "N/A",
            AssetCode = t.Asset != null ? t.Asset.AssetCode : "N/A",
            ProductName = t.Asset != null && t.Asset.Product != null ? t.Asset.Product.Name : "N/A",
            ToDivisionName = t.ToDivision != null ? t.ToDivision.Name : "N/A",
            TransferByName = t.TransferBy != null ? t.TransferBy.Username : "N/A",
            TargetUserId = t.TargetUser != null ? t.TargetUser.Id : 0,
            TargetUserName = t.TargetUser != null ? t.TargetUser.Username : "N/A",
            CurrentHolderId = t.CurrentHolderId,
            CurrentHolderName = t.CurrentHolder != null ? t.CurrentHolder.Username : null,
            Reason = t.Reason,
            TransferPeriod = t.TransferPeriod,
            Status = t.Status.ToString(),
            CreatedAt = t.CreatedAt
        }).ToListAsync();
    }
}