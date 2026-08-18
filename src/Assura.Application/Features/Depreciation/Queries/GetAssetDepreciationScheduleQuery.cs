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

public record GetAssetDepreciationScheduleQuery(int AssetId) : IRequest<AssetDepreciationScheduleDto?>;

public class GetAssetDepreciationScheduleQueryHandler : IRequestHandler<GetAssetDepreciationScheduleQuery, AssetDepreciationScheduleDto?>
{
    private readonly IApplicationDbContext _context;

    public GetAssetDepreciationScheduleQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AssetDepreciationScheduleDto?> Handle(GetAssetDepreciationScheduleQuery request, CancellationToken cancellationToken)
    {
        var asset = await _context.Assets
            .AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.Product)
            .FirstOrDefaultAsync(a => a.Id == request.AssetId && !a.IsDeleted, cancellationToken);

        if (asset == null)
        {
            return null;
        }

        var initialPrice = asset.PurchaseValue;
        var rate = asset.Category?.DepreciationRate > 0 ? asset.Category.DepreciationRate : 10.0m;
        var assetDate = asset.AssetDate == default ? asset.CreatedAt : asset.AssetDate;
        var purchaseYear = assetDate.Year;
        var currentYear = DateTime.UtcNow.Year;

        var usefulLifeYears = rate > 0 ? (int)Math.Ceiling(100m / rate) : 10;
        var annualDepreciation = initialPrice * (rate / 100m);

        var schedule = new List<AssetDepreciationScheduleRowDto>();

        decimal runningBookValue = initialPrice;
        decimal runningAccumulated = 0m;

        for (int year = 1; year <= usefulLifeYears + 1; year++)
        {
            int calYear = purchaseYear + (year - 1);
            decimal beginVal = runningBookValue;

            if (beginVal <= 0.001m)
            {
                // Value has reached $0
                schedule.Add(new AssetDepreciationScheduleRowDto
                {
                    YearNumber = year,
                    CalendarYear = calYear,
                    BeginningValue = 0m,
                    DepreciationExpense = 0m,
                    AccumulatedDepreciation = Math.Round(initialPrice, 2),
                    EndingValue = 0m,
                    IsCurrentYear = calYear == currentYear
                });
                break;
            }

            decimal depExpense = Math.Min(beginVal, annualDepreciation);
            runningAccumulated = Math.Min(initialPrice, runningAccumulated + depExpense);
            runningBookValue = Math.Max(0m, beginVal - depExpense);

            schedule.Add(new AssetDepreciationScheduleRowDto
            {
                YearNumber = year,
                CalendarYear = calYear,
                BeginningValue = Math.Round(beginVal, 2),
                DepreciationExpense = Math.Round(depExpense, 2),
                AccumulatedDepreciation = Math.Round(runningAccumulated, 2),
                EndingValue = Math.Round(runningBookValue, 2),
                IsCurrentYear = calYear == currentYear
            });

            if (runningBookValue <= 0.001m)
            {
                break;
            }
        }

        // Current status calculation
        var totalDays = (decimal)(DateTime.UtcNow - assetDate).TotalDays;
        var ageInYears = Math.Max(0m, totalDays / 365.25m);
        var curAccum = Math.Min(initialPrice, annualDepreciation * ageInYears);
        var curVal = Math.Max(0m, initialPrice - curAccum);

        return new AssetDepreciationScheduleDto
        {
            AssetId = asset.Id,
            AssetCode = asset.AssetCode,
            ProductName = asset.Product?.Name ?? asset.Notes ?? "Asset",
            CategoryName = asset.Category?.Name ?? "Uncategorized",
            DepreciationRate = Math.Round(rate, 2),
            PurchaseValue = Math.Round(initialPrice, 2),
            AssetDate = assetDate,
            CurrentValue = Math.Round(curVal, 2),
            AccumulatedDepreciation = Math.Round(curAccum, 2),
            UsefulLifeYears = usefulLifeYears,
            Schedule = schedule
        };
    }
}
