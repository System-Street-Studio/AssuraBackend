using MediatR;
using Assura.Application.Features.Transfers.DTOs;
using Microsoft.EntityFrameworkCore;
using Assura.Application.Common.Interfaces;

namespace Assura.Application.Features.Transfers.Queries;

// Query to get a specific transfer by its ID
public class GetTransferByIdQuery : IRequest<TransferDto>
{
    public int Id { get; set; }

    public GetTransferByIdQuery(int id)
    {
        Id = id;
    }
}

// Handler for GetTransferByIdQuery to retrieve a specific transfer by its ID
public class GetTransferByIdQueryHandler : IRequestHandler<GetTransferByIdQuery, TransferDto>
{
    private readonly IApplicationDbContext _context;

    public GetTransferByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TransferDto> Handle(GetTransferByIdQuery request, CancellationToken cancellationToken)
    {
       
        var transferDto = await _context.Transfers
            .Where(t => t.Id == request.Id)
            .Select(t => new TransferDto
            {
                Id = t.Id,
                TransferNumber = t.TransferNumber,
                TransferDate = t.TransferDate,
                ReturnDate = t.ReturnDate,
                Reason = t.Reason,
                TransferPeriod = t.TransferPeriod,
                Status = t.Status.ToString(),

                
                AssetId = t.AssetId,
                AssetTag = t.Asset.AssetTag ?? string.Empty,
                AssetCode = t.Asset.AssetCode,
                AssetStatus = t.Asset.Status.ToString(),
                ProductName = t.Asset.Product != null ? t.Asset.Product.Name : string.Empty,

                // Request
                AssetRequestId = t.AssetRequestId,

                // From Division
                FromDivisionId = t.FromDivisionId,
                FromDivisionName = t.FromDivision != null ? t.FromDivision.Name : string.Empty,

                // To Division
                ToDivisionId = t.ToDivisionId,
                ToDivisionName = t.ToDivision != null ? t.ToDivision.Name : string.Empty,

                // Users (Transfer By)
                TransferById = t.TransferById,
                TransferByName = t.TransferBy != null ? t.TransferBy.Username : string.Empty,

                // Target User
                TargetUserId = t.TargetUserId,
                TargetUserName = t.TargetUser != null ? t.TargetUser.Username : string.Empty,

                // Current Holder
                CurrentHolderId = t.CurrentHolderId,
                CurrentHolderName = t.CurrentHolder != null ? t.CurrentHolder.Username : string.Empty,

                // Audit
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (transferDto == null)
            throw new KeyNotFoundException($"Transfer with ID {request.Id} not found");

        return transferDto;
    }
}