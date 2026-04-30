using MediatR;
using Microsoft.EntityFrameworkCore;
using Assura.Application.Common.Interfaces;
using Assura.Application.Features.Transfers.Commands;
using Assura.Domain.Entities;
using Assura.Domain.Enums;

namespace Assura.Application.Features.Transfers.Handlers;

public class CreateTransferCommandHandler : IRequestHandler<CreateTransferCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateTransferCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateTransferCommand request, CancellationToken cancellationToken)
    {
        // 1️⃣ Get Asset
        var asset = await _context.Assets
            .Include (a => a.Product)
            .FirstOrDefaultAsync(a => a.Id == request.AssetId, cancellationToken);

        if (asset == null)
            throw new Exception($"Asset with ID {request.AssetId} not found");

        // 2️⃣ Get Asset Request (Optional)
        AssetRequest? assetRequest = null;

        if (request.AssetRequestId.HasValue)
        {
            assetRequest = await _context.AssetRequests
                .FirstOrDefaultAsync(a => a.Id == request.AssetRequestId.Value, cancellationToken);

            if (assetRequest == null)
                throw new Exception($"AssetRequest with ID {request.AssetRequestId} not found");
        }

        // 3️⃣ Extract Reason & Period
        string? reason = assetRequest?.Reason;
        string? transferPeriod = ExtractTransferPeriod(reason);
        string? cleanedReason = CleanReason(reason, transferPeriod);

        // 4️⃣ Current Holder
        User? currentHolder = null;

        if (asset.AssignedUserId.HasValue)
        {
            currentHolder = await _context.Users
                .Include(u => u.Division)
                .FirstOrDefaultAsync(u => u.Id == asset.AssignedUserId.Value, cancellationToken);
        }

        // 5️⃣ Target User (Requester)
        if (assetRequest == null)
            throw new Exception("Transfer must be linked to an AssetRequest");

        // Parse RequesterId - it might be stored as a string
        int targetUserId;
        if (int.TryParse(assetRequest.RequesterId.ToString(), out int parsedId))
        {
            targetUserId = parsedId;
        }
        else
        {
            throw new Exception($"Invalid RequesterId value: {assetRequest.RequesterId}");
        }

        var targetUser = await _context.Users
            .Include(u => u.Division)
            .FirstOrDefaultAsync(u => u.Id == targetUserId, cancellationToken);

        if (targetUser == null)
            throw new Exception("Target user not found");

        //  Transfer By
         User? transferBy = null;

        if (request.UserId.HasValue)
        {
            transferBy = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId.Value, cancellationToken);
        }

            

        // 6️⃣ Create Transfer
        var transfer = new Transfer
        {
            TransferNumber = GenerateTransferNumber(),

            AssetId = asset.Id,

            FromDivisionId = currentHolder?.DivisionId,
            FromDivision = currentHolder?.Division,

            CurrentHolderId = currentHolder?.Id,
            CurrentHolder = currentHolder,

            AssetRequestId = assetRequest.Id,
            AssetRequest = assetRequest,

            Reason = cleanedReason,
            TransferPeriod = transferPeriod,

            ToDivisionId = targetUser.DivisionId,
            ToDivision = targetUser.Division,

            TargetUserId = targetUser.Id,
            TargetUser = targetUser,

            
            TransferById = request.UserId,
            TransferBy = transferBy,

            TransferDate = DateTime.UtcNow,
            Status = TransferStatus.PendingOwnerApproval,

            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // 7️⃣ Save
        _context.Transfers.Add(transfer);
        await _context.SaveChangesAsync(cancellationToken);

        return transfer.Id;
    }

    // 🔢 Generate Transfer Number
    private string GenerateTransferNumber()
    {
        return $"TRF-{DateTime.UtcNow:yyyyMMddHHmmss}";
    }

    // 🔍 Extract Transfer Period
    private string? ExtractTransferPeriod(string? reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return null;

        var patterns = new[]
        {
            @"(\d+\s*(days?|weeks?|months?|years?))",
            @"(\d{1,2}/\d{1,2}/\d{2,4})\s*to\s*(\d{1,2}/\d{1,2}/\d{2,4})"
        };

        foreach (var pattern in patterns)
        {
            var match = System.Text.RegularExpressions.Regex.Match(
                reason, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (match.Success)
            {
                if (match.Groups.Count > 2)
                    return $"{match.Groups[1].Value} to {match.Groups[2].Value}";

                return match.Groups[1].Value;
            }
        }

        return null;
    }

    // 🧹 Clean Reason
    private string? CleanReason(string? reason, string? period)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return null;

        if (string.IsNullOrWhiteSpace(period))
            return reason.Trim();

        var cleaned = reason.Replace(period, "", StringComparison.OrdinalIgnoreCase);

        return cleaned.Trim();
    }
}