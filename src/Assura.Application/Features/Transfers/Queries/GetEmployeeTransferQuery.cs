using MediatR;
using Assura.Application.Features.Transfers.DTOs;
using Microsoft.EntityFrameworkCore;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;


namespace Assura.Application.Features.Transfers.Queries;

public record GetEmployeeTransferQuery(string Tab, int LoginUserId) : IRequest<List<TransferDto>>;

public class GetEmployeeTransferQueryHandler : IRequestHandler<GetEmployeeTransferQuery, List<TransferDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEmployeeTransferQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TransferDto>> Handle(GetEmployeeTransferQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Transfers
            .Include(t => t.Asset)
                .ThenInclude(a => a.Product)
            .Include(t => t.TargetUser)
            .Include(t => t.CurrentHolder)
            .Include(t => t.TransferBy)
            .Include(t => t.ToDivision)
            .AsNoTracking();

        query = request.Tab.ToLower() switch
        {
            // 1. Incoming: Status = PendingOwnerApproval AND I am the Current Holder
            "incoming" => query.Where(t => t.Status == TransferStatus.PendingOwnerApproval 
                                        && t.CurrentHolderId == request.LoginUserId),

            // 2. Pending: Status = PendingOwnerDivisionHeadApproval AND I am the Current Holder
            "pending" => query.Where(t => t.Status == TransferStatus.PendingOwnerDivisionHeadApproval || t.Status == TransferStatus.WaitingForFinalConfirmation
                                       && t.CurrentHolderId == request.LoginUserId),

            // 3. Active: Status = Active AND (I am Holder OR Target)
            "active" => query.Where(t => t.Status == TransferStatus.Active 
                                      && (t.CurrentHolderId == request.LoginUserId || t.TargetUserId == request.LoginUserId)),

            // 4. Completed: Status = Completed AND (I am Holder OR Target)
            "completed" => query.Where(t => t.Status == TransferStatus.Completed 
                                         && (t.CurrentHolderId == request.LoginUserId || t.TargetUserId == request.LoginUserId)),

            _ => query
        };

        return await query.Select(t => new TransferDto
        {
            Id = t.Id,
            AssetTag = (t.Asset != null ? t.Asset.AssetTag : null) ?? "N/A",
            AssetCode = t.Asset != null ? t.Asset.AssetCode : "N/A",
            ProductName = t.Asset != null && t.Asset.Product != null ? t.Asset.Product.Name : "N/A",
            ToDivisionName = (t.ToDivision != null ? t.ToDivision.Name : null) ?? "N/A",
            TransferByName = (t.TransferBy != null ? t.TransferBy.Username : null) ?? "N/A",
            TargetUserId = t.TargetUser != null ? t.TargetUser.Id : 0,
            TargetUserName = t.TargetUser != null ? t.TargetUser.Username : "N/A",
            CurrentHolderId = t.CurrentHolderId,
            CurrentHolderName = t.CurrentHolder != null ? t.CurrentHolder.Username : string.Empty,
            FromDivisionName = (t.FromDivision != null ? t.FromDivision.Name : null) ?? "N/A",
            Reason = t.Reason,
            TransferPeriod = t.TransferPeriod,
            TransferDate = t.TransferDate,
            ReturnDate = t.ReturnDate,
            Status = t.Status.ToString(),
            UpdatedAt = t.UpdatedAt,
            CreatedAt = t.CreatedAt
        }).ToListAsync(cancellationToken);
    }
}