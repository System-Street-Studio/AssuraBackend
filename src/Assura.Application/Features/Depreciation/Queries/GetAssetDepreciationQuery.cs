using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Assura.Application.Common.Interfaces;
using Assura.Application.Features.Depreciation.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Depreciation.Queries;

public record GetAssetDepreciationQuery(int? CategoryId = null, int? DivisionId = null, int? TargetYear = null) : IRequest<DepreciationSummaryDto>;

public class GetAssetDepreciationQueryHandler : IRequestHandler<GetAssetDepreciationQuery, DepreciationSummaryDto>
{
    private readonly IApplicationDbContext _context;

    public GetAssetDepreciationQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DepreciationSummaryDto> Handle(GetAssetDepreciationQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Assets
            .AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.Division)
            .Include(a => a.Product)
            .Where(a => !a.IsDeleted);

        if (request.CategoryId.HasValue && request.CategoryId.Value > 0)
        {
            query = query.Where(a => a.CategoryId == request.CategoryId.Value);
        }

        if (request.DivisionId.HasValue && request.DivisionId.Value > 0)
        {
            query = query.Where(a => a.DivisionId == request.DivisionId.Value);
        }

        var assets = await query.ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        DateTime evaluationDate;
        if (request.TargetYear.HasValue && request.TargetYear.Value > 1900)
        {
            // End of target year (Dec 31 23:59:59)
            evaluationDate = new DateTime(request.TargetYear.Value, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        }
        else
        {
            evaluationDate = now;
        }

        var assetDepreciationList = new List<AssetDepreciationDto>();

        foreach (var asset in assets)
        {
            var initialPrice = asset.PurchaseValue;
            var rate = asset.Category?.DepreciationRate > 0 ? asset.Category.DepreciationRate : 10.0m;

            var assetDate = asset.AssetDate == default ? asset.CreatedAt : asset.AssetDate;
            if (assetDate > evaluationDate)
            {
                // Asset not yet acquired at target date
                assetDepreciationList.Add(new AssetDepreciationDto
                {
                    Id = asset.Id,
                    AssetCode = asset.AssetCode,
                    AssetTag = asset.AssetTag,
                    SerialNumber = asset.SerialNumber,
                    ProductName = asset.Product?.Name ?? asset.Notes ?? "Asset",
                    CategoryId = asset.CategoryId,
                    CategoryName = asset.Category?.Name ?? "Uncategorized",
                    DivisionId = asset.DivisionId,
                    DivisionName = asset.Division?.Name ?? "Unassigned",
                    AssetDate = assetDate,
                    PurchaseValue = Math.Round(initialPrice, 2),
                    DepreciationRate = Math.Round(rate, 2),
                    AgeInYears = 0m,
                    CompletedYears = 0,
                    AnnualDepreciation = Math.Round(initialPrice * (rate / 100m), 2),
                    AccumulatedDepreciation = 0m,
                    CurrentValue = Math.Round(initialPrice, 2),
                    IsFullyDepreciated = false,
                    Status = "Future Acquisition",
                    UsefulLifeYears = rate > 0 ? (int)Math.Ceiling(100m / rate) : 10
                });
                continue;
            }

            var totalDays = (decimal)(evaluationDate - assetDate).TotalDays;
            var ageInYears = Math.Max(0m, totalDays / 365.25m);
            var completedYears = (int)Math.Floor(ageInYears);

            var annualDepreciation = initialPrice * (rate / 100m);
            var accumulatedDepreciation = Math.Min(initialPrice, Math.Max(0m, annualDepreciation * ageInYears));
            var currentValue = Math.Max(0m, initialPrice - accumulatedDepreciation);
            var isFullyDepreciated = currentValue <= 0.001m;

            var usefulLife = rate > 0 ? (int)Math.Ceiling(100m / rate) : 10;

            string statusText = isFullyDepreciated ? "Fully Depreciated" : "Active";
            if (asset.Status.ToString().Contains("Discarded", StringComparison.OrdinalIgnoreCase))
            {
                statusText = "Discarded";
            }

            assetDepreciationList.Add(new AssetDepreciationDto
            {
                Id = asset.Id,
                AssetCode = asset.AssetCode,
                AssetTag = asset.AssetTag,
                SerialNumber = asset.SerialNumber,
                ProductName = asset.Product?.Name ?? asset.Notes ?? "Asset",
                CategoryId = asset.CategoryId,
                CategoryName = asset.Category?.Name ?? "Uncategorized",
                DivisionId = asset.DivisionId,
                DivisionName = asset.Division?.Name ?? "Unassigned",
                AssetDate = assetDate,
                PurchaseValue = Math.Round(initialPrice, 2),
                DepreciationRate = Math.Round(rate, 2),
                AgeInYears = Math.Round(ageInYears, 2),
                CompletedYears = completedYears,
                AnnualDepreciation = Math.Round(annualDepreciation, 2),
                AccumulatedDepreciation = Math.Round(accumulatedDepreciation, 2),
                CurrentValue = Math.Round(currentValue, 2),
                IsFullyDepreciated = isFullyDepreciated,
                Status = statusText,
                UsefulLifeYears = usefulLife
            });
        }

        // Calculate summary statistics
        var totalPurchaseValue = assetDepreciationList.Sum(a => a.PurchaseValue);
        var totalAccumulatedDepreciation = assetDepreciationList.Sum(a => a.AccumulatedDepreciation);
        var totalCurrentValue = assetDepreciationList.Sum(a => a.CurrentValue);
        var fullyDepreciatedCount = assetDepreciationList.Count(a => a.IsFullyDepreciated);
        var activeDepreciatingCount = assetDepreciationList.Count(a => !a.IsFullyDepreciated);

        var overallPercentage = totalPurchaseValue > 0
            ? Math.Round((totalAccumulatedDepreciation / totalPurchaseValue) * 100m, 2)
            : 0m;

        // Group by Category
        var categorySummaries = assetDepreciationList
            .GroupBy(a => new { a.CategoryId, a.CategoryName, a.DepreciationRate })
            .Select(g => new CategoryDepreciationSummaryDto
            {
                CategoryId = g.Key.CategoryId ?? 0,
                CategoryName = g.Key.CategoryName ?? "Uncategorized",
                DepreciationRate = g.Key.DepreciationRate,
                TotalAssets = g.Count(),
                TotalPurchaseValue = Math.Round(g.Sum(x => x.PurchaseValue), 2),
                TotalAccumulatedDepreciation = Math.Round(g.Sum(x => x.AccumulatedDepreciation), 2),
                TotalCurrentValue = Math.Round(g.Sum(x => x.CurrentValue), 2),
                FullyDepreciatedCount = g.Count(x => x.IsFullyDepreciated)
            })
            .OrderBy(c => c.CategoryName)
            .ToList();

        return new DepreciationSummaryDto
        {
            TotalAssets = assetDepreciationList.Count,
            TotalPurchaseValue = Math.Round(totalPurchaseValue, 2),
            TotalAccumulatedDepreciation = Math.Round(totalAccumulatedDepreciation, 2),
            TotalCurrentValue = Math.Round(totalCurrentValue, 2),
            FullyDepreciatedAssets = fullyDepreciatedCount,
            ActiveDepreciatingAssets = activeDepreciatingCount,
            OverallDepreciationPercentage = overallPercentage,
            Assets = assetDepreciationList.OrderByDescending(a => a.PurchaseValue).ToList(),
            CategoryBreakdown = categorySummaries
        };
    }
}
