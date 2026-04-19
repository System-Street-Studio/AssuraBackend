using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Requests.Queries;

public record GetSuggestedAssetsForRequestQuery(int RequestId) : IRequest<List<SuggestedAssetDto>>;

public class SuggestedAssetDto
{
    public int Id { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public int Score { get; set; }
}

public class GetSuggestedAssetsForRequestQueryHandler : IRequestHandler<GetSuggestedAssetsForRequestQuery, List<SuggestedAssetDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSuggestedAssetsForRequestQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SuggestedAssetDto>> Handle(GetSuggestedAssetsForRequestQuery request, CancellationToken cancellationToken)
    {
        var req = await _context.Requests
            .AsNoTracking()
            .Include(r => r.Asset)
                .ThenInclude(a => a!.Product)
            .Include(r => r.Asset)
                .ThenInclude(a => a!.Category)
            .FirstOrDefaultAsync(r => r.Id == request.RequestId, cancellationToken);

        if (req == null)
        {
            return new List<SuggestedAssetDto>();
        }

        var normalizedDescription = BuildNormalizedText(req.Description, req.Specifications, req.SpecialNote);
        var nowUtc = DateTime.UtcNow;

        var candidates = await _context.Assets
            .AsNoTracking()
            .Include(a => a.Product)
            .Include(a => a.Category)
            .Where(a => a.Status == AssetStatus.InStore && a.AssignedUserId == null)
            .Where(a => !a.ReservedForUserId.HasValue || (a.ReservedUntilUtc.HasValue && a.ReservedUntilUtc.Value < nowUtc))
            .ToListAsync(cancellationToken);

        var requestedAsset = req.Asset;
        var scored = candidates
            .Select(asset => new SuggestedAssetDto
            {
                Id = asset.Id,
                AssetCode = asset.AssetCode,
                ProductName = asset.Product?.Name ?? "N/A",
                CategoryName = asset.Category?.Name ?? "N/A",
                SerialNumber = asset.SerialNumber,
                Score = ScoreCandidate(asset, requestedAsset, normalizedDescription)
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.AssetCode)
            .Take(12)
            .ToList();

        return scored;
    }

    private static int ScoreCandidate(Asset candidate, Asset? requestedAsset, string normalizedDescription)
    {
        var score = 5;

        if (requestedAsset != null)
        {
            if (candidate.Id == requestedAsset.Id)
            {
                score += 120;
            }

            if (candidate.ProductId == requestedAsset.ProductId)
            {
                score += 90;
            }

            if (candidate.CategoryId == requestedAsset.CategoryId)
            {
                score += 55;
            }
        }

        var productName = candidate.Product?.Name ?? string.Empty;
        var categoryName = candidate.Category?.Name ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(productName) && normalizedDescription.Contains(productName, StringComparison.OrdinalIgnoreCase))
        {
            score += 40;
        }

        if (!string.IsNullOrWhiteSpace(categoryName) && normalizedDescription.Contains(categoryName, StringComparison.OrdinalIgnoreCase))
        {
            score += 25;
        }

        if (!string.IsNullOrWhiteSpace(candidate.AssetCode) && normalizedDescription.Contains(candidate.AssetCode, StringComparison.OrdinalIgnoreCase))
        {
            score += 25;
        }

        if (!string.IsNullOrWhiteSpace(candidate.SerialNumber) && normalizedDescription.Contains(candidate.SerialNumber, StringComparison.OrdinalIgnoreCase))
        {
            score += 15;
        }

        return score;
    }

    private static string BuildNormalizedText(params string?[] values)
    {
        return string.Join(" ", values.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v!.Trim()));
    }
}
