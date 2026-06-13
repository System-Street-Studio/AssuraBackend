using MediatR;
using Microsoft.EntityFrameworkCore;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using Assura.Application.Features.Transfers.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace Assura.Application.Features.Transfers.Queries;


public record GetTransferCountsQuery(int LoginUserId) : IRequest<TransferCountsDto>;


public class GetTransferCountsQueryHandler : IRequestHandler<GetTransferCountsQuery, TransferCountsDto>
{
    private readonly IApplicationDbContext _context;

    public GetTransferCountsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TransferCountsDto> Handle(GetTransferCountsQuery request, CancellationToken cancellationToken)
    {
     
        var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.LoginUserId, cancellationToken);
        
        if (currentUser == null)
            return new TransferCountsDto();
        
        var headDivisionId = currentUser.DivisionId;


        
        var outgoingCount = await _context.Transfers
            .CountAsync(t => t.ToDivisionId == headDivisionId 
                          && t.Status == TransferStatus.PendingOwnerApproval, cancellationToken);

        var incomingCount = await _context.Transfers
            .CountAsync(t => t.FromDivisionId == headDivisionId 
                          && t.Status == TransferStatus.PendingOwnerDivisionHeadApproval, cancellationToken);

        var pendingCount = await _context.Transfers
            .CountAsync(t => t.ToDivisionId == headDivisionId 
                          && t.Status == TransferStatus.WaitingForFinalConfirmation, cancellationToken);

        var activeCount = await _context.Transfers
            .CountAsync(t => t.Status == TransferStatus.Active 
                          && (t.FromDivisionId == headDivisionId || t.ToDivisionId == headDivisionId), cancellationToken);

        var completedCount = await _context.Transfers
            .CountAsync(t => t.Status == TransferStatus.Completed 
                          && (t.FromDivisionId == headDivisionId || t.ToDivisionId == headDivisionId), cancellationToken);

        
        return new TransferCountsDto
        {
            OutgoingCount = outgoingCount,
            IncomingCount = incomingCount,
            PendingCount = pendingCount,
            ActiveCount = activeCount,
            CompletedCount = completedCount
        };
    }
}