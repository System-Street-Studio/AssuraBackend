using MediatR;
using Microsoft.EntityFrameworkCore;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Assura.Domain.Enums;

namespace Assura.Application.Features.Transfers.Commands;

// 1. Command
public class CreateTransferCommand : IRequest<int>
{
    public int AssetId { get; set; }
    public int AssetRequestId { get; set; }
    public int UserId { get; set; }
}

// 2. Command Handler
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
    // --- 1. Get and Validate Asset ---
    var asset = await _context.Assets
        .Where(a => a.Id == request.AssetId)
        .Select(a => new 
        {
            Id = a.Id,
            AssetCode = a.AssetCode,
            Status = a.Status,
            AssignedUserId = a.AssignedUserId
        })
        .FirstOrDefaultAsync(cancellationToken);

    if (asset == null)
        throw new KeyNotFoundException($"Asset with ID {request.AssetId} not found.");

    if (asset.AssignedUserId == null)
        throw new InvalidOperationException($"AssetId {request.AssetId} is not currently assigned to any employee.");

    if (asset.Status == AssetStatus.UnderMaintenance)
        throw new InvalidOperationException($"Asset {asset.AssetCode} is under maintenance and cannot be transferred.");

    // --- 2. Get and Validate Asset Request ---
    var assetRequest = await _context.AssetRequests
        .Where(a => a.Id == request.AssetRequestId)
        .Select(a => new 
        {
            Id = a.Id,
            RequesterId = a.RequesterId,
            Reason = a.Reason
        })
        .FirstOrDefaultAsync(cancellationToken);

    if (assetRequest == null)
        throw new KeyNotFoundException($"AssetRequest with ID {request.AssetRequestId} not found.");

    // --- 3. Extract Reason & Period ---
    string? reason = assetRequest.Reason;
    string? transferPeriod = ExtractTransferPeriod(reason);
    string? cleanedReason = ExtractTransferReason(reason);

    // --- 4. Get Current Holder ---
    var currentHolder = await _context.Users
        .Where(u => u.Id == asset.AssignedUserId.Value)
        .Select(u => new 
        {
            Id = u.Id,
            DivisionId = u.DivisionId
        })
        .FirstOrDefaultAsync(cancellationToken);
    
    if (currentHolder == null)
        throw new KeyNotFoundException($"Current holder user with ID {asset.AssignedUserId.Value} not found.");

    if (currentHolder.DivisionId == null)
        throw new InvalidOperationException($"Current holder does not belong to any division.");

    // --- 5. Validate Target User (From Request) ---
    if (string.IsNullOrWhiteSpace(assetRequest.RequesterId?.ToString()))
    {
        throw new InvalidOperationException("RequesterId is missing in the Asset Request.");
    }

    if (!int.TryParse(assetRequest.RequesterId.ToString(), out int targetUserId))
    {
        throw new InvalidOperationException($"RequesterId '{assetRequest.RequesterId}' is not a valid integer ID.");
    }


    var targetUser = await _context.Users
        .Where(u => u.Id == targetUserId)
        .Select(u => new 
        {
            Id = u.Id,
            DivisionId = u.DivisionId
        })
        .FirstOrDefaultAsync(cancellationToken);

    if (targetUser == null)
        throw new KeyNotFoundException($"Target user with ID {targetUserId} not found.");

    if (targetUser.DivisionId == null)
        throw new InvalidOperationException($"Target user does not belong to any division.");

    // --- 6. Get Transfer Performed By User ---

    var transferBy = await _context.Users
        .Where(u => u.Id == request.UserId)
        .Select(u => new { Id = u.Id, Role = u.Role, DivisionId = u.DivisionId })
        .FirstOrDefaultAsync(cancellationToken);

    if (transferBy == null)
        throw new KeyNotFoundException($"Calling user with ID {request.UserId} not found.");

    // The caller here is whichever Division Head approved the underlying (Transfer-type)
    // AssetRequest and is now fulfilling it from the Asset Pool — i.e. the *target*
    // user's (requester's) Division Head, not the current asset holder's. This matches
    // GetApprovedTransfersQueryHandler, which scopes the pool's dropdown to
    // AssetRequest.DivisionId == caller's own DivisionId, and AssetRequest.DivisionId is
    // set to the requester's division. The current holder's division is frequently a
    // *different* division — that's the whole point of a transfer — so scoping against
    // it here would reject every legitimate cross-division transfer. Admins are exempt
    // since they legitimately act across divisions.
    if (transferBy.Role != UserRole.Admin && transferBy.DivisionId != targetUser.DivisionId)
    {
        throw new UnauthorizedAccessException("You may only initiate transfers for requests approved within your own division.");
    }

    // --- 7. Create New Transfer Instance (Safe Mapping) ---
    var transfer = new Transfer
    {
        TransferNumber = GenerateTransferNumber(),
        AssetId = asset.Id,
        AssetRequestId = assetRequest.Id, 

        FromDivisionId = currentHolder.DivisionId.Value,
        CurrentHolderId = currentHolder.Id,
        
        ToDivisionId = targetUser.DivisionId.Value,
        TargetUserId = targetUser.Id,
        
        TransferById = transferBy?.Id,

        Reason = cleanedReason,
        TransferPeriod = transferPeriod,
        TransferDate = DateTime.UtcNow,
        Status = TransferStatus.PendingOwnerApproval,

        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    // --- 8. Save to Database ---
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

        var match = System.Text.RegularExpressions.Regex.Match(
            reason, @"^(.*?)\s*\(Transfer periods:", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        return match.Success ? match.Groups[1].Value.Trim() : reason.Trim();
    }

    // Extract Transfer Period
    private string? ExtractTransferPeriod(string? reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return null;

        var match = System.Text.RegularExpressions.Regex.Match(
            reason, @"\(Transfer periods:(.*?)\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        return match.Success ? match.Groups[1].Value.Trim() : null;
    }
}