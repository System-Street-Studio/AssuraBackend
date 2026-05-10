using MediatR;
using Microsoft.EntityFrameworkCore;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Assura.Domain.Enums;

namespace Assura.Application.Features.Transfers.Commands;

//command
public class CreateTransferCommand : IRequest<int>
{
    public int AssetId { get; set; }
    public int AssetRequestId { get; set; }
    public int UserId { get; set; }
}

//command Handler
public class CreateTransferCommandHandler : IRequestHandler<CreateTransferCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateTransferCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

// Handle the command to create a new transfer request
    public async Task<int> Handle(CreateTransferCommand request, CancellationToken cancellationToken)
    {
        //  Get Asset
        var asset = await _context.Assets
            .Include (a => a.Product)
            .FirstOrDefaultAsync(a => a.Id == request.AssetId, cancellationToken);

        if (asset == null)
            throw new Exception($"Asset not found or not assigned");

        // Validation: Asset must be assigned
        if (!asset.AssignedUserId.HasValue)
            throw new InvalidOperationException($"AssetId {request.AssetId} does not exist or is not assigned to an employee");



        //  Get Asset Request
        var assetRequest = await _context.AssetRequests
            .FirstOrDefaultAsync(a => a.Id == request.AssetRequestId, cancellationToken);

        if (assetRequest == null)
            throw new KeyNotFoundException($"AssetRequest with ID {request.AssetRequestId} not found.");



        //  Extract Reason & Period
        string? reason = assetRequest?.Reason;
        string? transferPeriod = ExtractTransferPeriod(reason);
        string? cleanedReason = ExtractTransferReason(reason);

 
 
        // Current Holder
        User? currentHolder = null;

        if (asset.AssignedUserId.HasValue)
        {
            currentHolder = await _context.Users
                .Include(u => u.Division)
                .FirstOrDefaultAsync(u => u.Id == asset.AssignedUserId.Value, cancellationToken);
        }


       // Targert user
 
        if (string.IsNullOrWhiteSpace(assetRequest?.RequesterId?.ToString()))
        {
            throw new InvalidOperationException("RequesterId is missing in the Asset Request.");
        }

        if (!int.TryParse(assetRequest.RequesterId.ToString(), out int targetUserId))
        {
            throw new InvalidOperationException($"RequesterId '{assetRequest.RequesterId}' is not a valid integer ID.");
        }

        var targetUser = await _context.Users
            .Include(u => u.Division)
            .FirstOrDefaultAsync(u => u.Id == targetUserId, cancellationToken);

        if (targetUser == null)
            throw new KeyNotFoundException($"Target user with ID {targetUserId} not found.");





        //  Transfer By
      
         var transferBy = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        

            

        //  Create Transfer
        var transfer = new Transfer
        {
            TransferNumber = GenerateTransferNumber(),

            AssetId = asset.Id,
            Asset = asset,

            FromDivisionId = currentHolder?.DivisionId,
            FromDivision = currentHolder?.Division,

            CurrentHolderId = currentHolder?.Id,
            CurrentHolder = currentHolder,

            AssetRequestId = assetRequest.Id,
            AssetRequest = assetRequest,

            Reason = cleanedReason,
            TransferPeriod = transferPeriod,

            ToDivisionId = targetUser?.DivisionId,
            ToDivision = targetUser?.Division,

            TargetUserId = targetUser?.Id,
            TargetUser = targetUser,

            
            TransferById = request.UserId,
            TransferBy = transferBy,

            TransferDate = DateTime.UtcNow,
            Status = TransferStatus.PendingOwnerApproval,

            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Save
        _context.Transfers.Add(transfer);
        await _context.SaveChangesAsync(cancellationToken);

        return transfer.Id;
    }


    // Generate Transfer Number
    private string GenerateTransferNumber()
    {
        return $"TRF-{DateTime.UtcNow:yyyyMMddHHmmss}";
    }


    // Extract Transfer Reason 
    private string? ExtractTransferReason(string? reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return null;

        // Match text before "(Transfer periods:"
        var match = System.Text.RegularExpressions.Regex.Match(
            reason, @"^(.*?)\s*\(Transfer periods:", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        return match.Success ? match.Groups[1].Value.Trim() : reason.Trim();
    }


    // Extract Transfer Period (the period part including dates)
    private string? ExtractTransferPeriod(string? reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return null;

        // Match the period part "(Transfer periods: ...)"
        var match = System.Text.RegularExpressions.Regex.Match(
            reason, @"\(Transfer periods:(.*?)\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        return null;
    }
}