using MediatR;
using Microsoft.EntityFrameworkCore;
using Assura.Application.Features.Transfers.Commands;
using Assura.Domain.Entities;
using Assura.Application.Common.Interfaces;
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
        Console.WriteLine("🔄 === TRANSFER CREATION STARTED ===");

        // 1️⃣ Get Asset
        var asset = await _context.Assets
            .FirstOrDefaultAsync(a => a.Id == request.AssetId, cancellationToken);

        if (asset == null)
            throw new Exception($"Asset with ID {request.AssetId} not found");

        Console.WriteLine($"📦 Asset: {asset.AssetTag}");

        
        // 2️⃣ Get AssetRequest
        AssetRequest? assetRequest = null;

        if (request.AssetRequestId > 0)
        {
            assetRequest = await _context.AssetRequests
                .FirstOrDefaultAsync(a => a.Id == request.AssetRequestId, cancellationToken);

            if (assetRequest == null)
                throw new Exception($"AssetRequest with ID {request.AssetRequestId} not found");
        }

        
        // 3️⃣ Extract Reason & TransferPeriod
        string? reason = assetRequest?.Reason;
        string? transferPeriod = ExtractTransferPeriod(reason);
        string? cleanedReason = CleanReason(reason, transferPeriod);

        Console.WriteLine($"📝 Original Reason: {reason}");
        Console.WriteLine($"⏳ Transfer Period: {transferPeriod}");
        Console.WriteLine($"🧹 Cleaned Reason: {cleanedReason}");

        
        
        // 4️⃣ Current Holder
        User? currentHolder = null;

        if (asset.AssignedUserId.HasValue)
        {
            currentHolder = await _context.Users
                .Include(u => u.Division)
                .FirstOrDefaultAsync(u => u.Id == asset.AssignedUserId.Value, cancellationToken);
        }

        
        // 5 Target User
        int targetUserId;

        if (assetRequest != null && int.TryParse(assetRequest.RequesterId, out int parsedId))
        {
            targetUserId = parsedId;
        }
        else
        {
            throw new Exception("Target user cannot be determined from request");
        }

        var targetUser = await _context.Users
            .Include(u => u.Division)
            .FirstOrDefaultAsync(u => u.Id == targetUserId, cancellationToken);

        if (targetUser == null)
        {
            Console.WriteLine($" Target user not found for ID: {targetUserId}");
            throw new Exception("Target user not found");
        }

        Console.WriteLine($"👤 Target User: {targetUser.FirstName}");



        // 6 Create Transfer
        var transfer = new Transfer
        {
            TransferNumber = $"TRF{Guid.NewGuid():N}",
            AssetId = asset.Id,
            AssetTag = asset.AssetTag,
            AssetName = asset.Product?.Name ?? "Unknown",

            FromDivisionId = currentHolder?.DivisionId ?? 0,
            FromDivision = currentHolder?.Division,

            CurrentHolderId = currentHolder?.Id,
            CurrentHolder = currentHolder,

            AssetRequestId = assetRequest?.Id ?? 0,
            AssetRequest = assetRequest,
            Reason = cleanedReason,

            TransferPeriod = transferPeriod,

            ToDivisionId = targetUser.DivisionId ?? 0,
            ToDivision = targetUser.Division,

            TargetUserId = targetUser.Id,
            TargetUser = targetUser,

            TransferById = targetUser.Id,
            TransferBy = targetUser,
            
            TransferDate = DateTime.UtcNow,
           
            Status = TransferStatus.PendingOwnerApproval,

            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // 7️⃣ Save
        _context.Transfers.Add(transfer);
        await _context.SaveChangesAsync(cancellationToken);

        Console.WriteLine($"✅ Transfer Created ID: {transfer.Id}");

        return transfer.Id;
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
            var match = System.Text.RegularExpressions.Regex.Match(reason, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

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