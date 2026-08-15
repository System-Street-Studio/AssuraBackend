using System;
using System.Collections.Generic;

namespace Assura.Application.Features.Depreciation.DTOs;

public class AssetDepreciationDto
{
    public int Id { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string? AssetTag { get; set; }
    public string? SerialNumber { get; set; }
    public string? ProductName { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int? DivisionId { get; set; }
    public string? DivisionName { get; set; }
    public DateTime AssetDate { get; set; }
    public decimal PurchaseValue { get; set; }
    public decimal DepreciationRate { get; set; }
    public decimal AgeInYears { get; set; }
    public int CompletedYears { get; set; }
    public decimal AnnualDepreciation { get; set; }
    public decimal AccumulatedDepreciation { get; set; }
    public decimal CurrentValue { get; set; }
    public bool IsFullyDepreciated { get; set; }
    public string Status { get; set; } = string.Empty;
    public int UsefulLifeYears { get; set; }
}

public class CategoryDepreciationSummaryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal DepreciationRate { get; set; }
    public int TotalAssets { get; set; }
    public decimal TotalPurchaseValue { get; set; }
    public decimal TotalAccumulatedDepreciation { get; set; }
    public decimal TotalCurrentValue { get; set; }
    public int FullyDepreciatedCount { get; set; }
}

public class DepreciationSummaryDto
{
    public int TotalAssets { get; set; }
    public decimal TotalPurchaseValue { get; set; }
    public decimal TotalAccumulatedDepreciation { get; set; }
    public decimal TotalCurrentValue { get; set; }
    public int FullyDepreciatedAssets { get; set; }
    public int ActiveDepreciatingAssets { get; set; }
    public decimal OverallDepreciationPercentage { get; set; }
    public List<AssetDepreciationDto> Assets { get; set; } = new();
    public List<CategoryDepreciationSummaryDto> CategoryBreakdown { get; set; } = new();
}

public class AssetDepreciationScheduleRowDto
{
    public int YearNumber { get; set; }
    public int CalendarYear { get; set; }
    public decimal BeginningValue { get; set; }
    public decimal DepreciationExpense { get; set; }
    public decimal AccumulatedDepreciation { get; set; }
    public decimal EndingValue { get; set; }
    public bool IsCurrentYear { get; set; }
}

public class AssetDepreciationScheduleDto
{
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string? ProductName { get; set; }
    public string? CategoryName { get; set; }
    public decimal DepreciationRate { get; set; }
    public decimal PurchaseValue { get; set; }
    public DateTime AssetDate { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal AccumulatedDepreciation { get; set; }
    public int UsefulLifeYears { get; set; }
    public List<AssetDepreciationScheduleRowDto> Schedule { get; set; } = new();
}
