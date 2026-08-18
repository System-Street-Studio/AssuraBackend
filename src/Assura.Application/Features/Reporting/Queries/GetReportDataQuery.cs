using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Assura.Application.Features.Reporting.Queries;

public record GetReportDataQuery(string ReportType, DateTime? StartDate = null, DateTime? EndDate = null, int? DivisionId = null) : IRequest<List<Dictionary<string, object>>>;

public class GetReportDataQueryHandler : IRequestHandler<GetReportDataQuery, List<Dictionary<string, object>>>
{
    private readonly IApplicationDbContext _context;

    public GetReportDataQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Dictionary<string, object>>> Handle(GetReportDataQuery request, CancellationToken cancellationToken)
    {
        var result = new List<Dictionary<string, object>>();

        var assetsList = new List<dynamic>();

        var query = _context.Assets
            .AsNoTracking()
            .Where(a => !a.IsDeleted);

        if (request.DivisionId.HasValue && request.DivisionId.Value > 0)
        {
            query = query.Where(a => a.DivisionId == request.DivisionId.Value);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(a => a.AssetDate >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(a => a.AssetDate <= request.EndDate.Value);
        }

        var assets = await query
            .Select(a => new
            {
                a.AssetCode,
                CategoryName = a.Category != null ? a.Category.Name : null,
                DivisionName = a.Division != null ? a.Division.Name : null,
                Status = (AssetStatus?)a.Status,
                a.LastVerifiedAt,
                a.Notes,
                a.AssetDate,
                a.PurchaseValue
            })
            .ToListAsync(cancellationToken);

        assetsList.AddRange(assets);

        var type = request.ReportType?.ToLower() ?? "audit";

        foreach (var asset in assetsList)
        {
            var row = new Dictionary<string, object>();
            
            if (type == "audit")
            {
                row["Asset Code"] = asset.AssetCode;
                row["Category"] = asset.CategoryName ?? "N/A";
                row["Division"] = asset.DivisionName ?? "N/A";
                row["Status"] = asset.Status?.ToString() ?? "Unknown";
                row["Last Verified At"] = asset.LastVerifiedAt?.ToString("yyyy-MM-dd") ?? "Never";
                result.Add(row);
            }
            else if (type == "exception")
            {
                if (asset.Status == AssetStatus.Lost || asset.Status == AssetStatus.Discarded || asset.LastVerifiedAt == null)
                {
                    row["Asset Code"] = asset.AssetCode;
                    row["Category"] = asset.CategoryName ?? "N/A";
                    row["Issue Type"] = asset.Status == AssetStatus.Lost ? "Lost" : asset.LastVerifiedAt == null ? "Unverified" : "Discarded";
                    row["Division"] = asset.DivisionName ?? "N/A";
                    row["Notes"] = asset.Notes ?? "No additional notes";
                    result.Add(row);
                }
            }
            else if (type == "lifecycle")
            {
                if (asset.Status == AssetStatus.Discarded || asset.Status == AssetStatus.Lost)
                {
                    row["Asset Code"] = asset.AssetCode;
                    row["Purchase Date"] = asset.AssetDate.ToString("yyyy-MM-dd");
                    row["Disposal Status"] = asset.Status?.ToString() ?? "Unknown";
                    row["Category"] = asset.CategoryName ?? "N/A";
                    row["Original Value"] = asset.PurchaseValue;
                    result.Add(row);
                }
            }
            else if (type == "finance")
            {
                row["Asset Code"] = asset.AssetCode;
                row["Category"] = asset.CategoryName ?? "N/A";
                row["Purchase Date"] = asset.AssetDate.ToString("yyyy-MM-dd");
                row["Purchase Value"] = asset.PurchaseValue;
                
                // Simple straight-line depreciation estimate (10% per year for example purposes)
                var ageInYears = (DateTime.UtcNow - asset.AssetDate).TotalDays / 365.25;
                var depreciation = Math.Min((decimal)(ageInYears * 0.1), 1.0m) * asset.PurchaseValue;
                row["Estimated Depreciation"] = Math.Round(depreciation, 2);
                row["Current Book Value"] = Math.Round(asset.PurchaseValue - depreciation, 2);
                
                result.Add(row);
            }
            else
            {
                // Default fallback to basic list
                row["Asset Code"] = asset.AssetCode;
                row["Category"] = asset.CategoryName ?? "N/A";
                row["Division"] = asset.DivisionName ?? "N/A";
                row["Status"] = asset.Status?.ToString() ?? "Unknown";
                result.Add(row);
            }
        }

        return result;
    }
}
