using MediatR;
using Assura.Application.Features.Transfers.DTOs;
using Microsoft.EntityFrameworkCore;
using Assura.Application.Common.Interfaces;

namespace Assura.Application.Features.Transfers.Queries;

//  Query to get a specific transfer by its ID
public class GetTransferByIdQuery : IRequest<TransferDto>
{
    public int Id { get; set; }

    public GetTransferByIdQuery(int id)
    {
        Id = id;
    }
}

//  Handler for GetTransferByIdQuery to retrieve a specific transfer by its ID
public class GetTransferByIdQueryHandler : IRequestHandler<GetTransferByIdQuery, TransferDto>
{
    private readonly IApplicationDbContext _context;

    public GetTransferByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TransferDto> Handle(GetTransferByIdQuery request, CancellationToken cancellationToken)
    {
        var transfer = await _context.Transfers
            .Include(t => t.Asset)
                .ThenInclude(a => a.Product)
            .Include(t => t.FromDivision)
            .Include(t => t.ToDivision)
            .Include(t => t.TransferBy)
            .Include(t => t.TargetUser)
            .Include(t => t.CurrentHolder)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (transfer == null)
            throw new KeyNotFoundException($"Transfer with ID {request.Id} not found");

        return new TransferDto
        {
            Id = transfer.Id,
            TransferNumber = transfer.TransferNumber,
            TransferDate = transfer.TransferDate,
            ReturnDate = transfer.ReturnDate,

            Reason = transfer.Reason,
            TransferPeriod = transfer.TransferPeriod,

            Status = transfer.Status.ToString(),

            // Asset
            AssetId = transfer.AssetId,
            AssetTag = transfer.Asset.AssetTag ?? string.Empty,
            AssetCode = transfer.Asset.AssetCode,
            AssetStatus = transfer.Asset.Status.ToString(),
            ProductName = transfer.Asset.Product?.Name ?? string.Empty,

            // Request
            AssetRequestId = transfer.AssetRequestId,

            // From Division
            FromDivisionId = transfer.FromDivisionId,
            FromDivisionName = transfer.FromDivision?.Name ?? string.Empty,

            // To Division
            ToDivisionId = transfer.ToDivisionId,
            ToDivisionName = transfer.ToDivision?.Name ?? string.Empty,

            // Users
            TransferById = transfer.TransferById,
            TransferByName = transfer.TransferBy?.Username ?? string.Empty,

            // Target User
            TargetUserId = transfer.TargetUserId ?? 0,
            TargetUserName = transfer.TargetUser?.Username ?? string.Empty,

            // Current Holder
            CurrentHolderId = transfer.CurrentHolderId,
            CurrentHolderName = transfer.CurrentHolder?.Username ?? string.Empty,

            // Audit
            CreatedAt = transfer.CreatedAt,
            UpdatedAt = transfer.UpdatedAt
        };
    }
}
