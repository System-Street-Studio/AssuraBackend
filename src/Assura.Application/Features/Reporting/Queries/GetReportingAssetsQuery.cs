using Assura.Application.Common.Interfaces;
using Assura.Application.Features.Reporting.DTOs;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Reporting.Queries;

public record GetReportingAssetsQuery(int PageNumber = 1, int PageSize = 20, string? SearchTerm = null) : IRequest<ReportingAssetsPageDto>;

public class GetReportingAssetsQueryHandler : IRequestHandler<GetReportingAssetsQuery, ReportingAssetsPageDto>
{
    private readonly IApplicationDbContext _context;

    public GetReportingAssetsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReportingAssetsPageDto> Handle(GetReportingAssetsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Assets
            .AsNoTracking()
            .Where(a => !a.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToUpper();
            query = query.Where(a =>
                a.AssetCode.ToUpper().Contains(term) ||
                (a.AssetTag != null && a.AssetTag.ToUpper().Contains(term)) ||
                (a.SerialNumber != null && a.SerialNumber.ToUpper().Contains(term)) ||
                (a.Product != null && a.Product.Name.ToUpper().Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var assets = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new
            {
                a.Id,
                a.AssetCode,
                a.AssetTag,
                a.AssetDate,
                Status = (AssetStatus?)a.Status,
                a.SerialNumber,
                a.Warranty,
                ProductName = a.Product != null ? a.Product.Name : null,
                DivisionName = a.Division != null ? a.Division.Name : null,
                AssignedFirstName = a.AssignedUser != null ? a.AssignedUser.FirstName : null,
                AssignedLastName = a.AssignedUser != null ? a.AssignedUser.LastName : null,
            })
            .ToListAsync(cancellationToken);

        var assetReferences = assets
            .SelectMany(asset => new[] { asset.Id.ToString(), asset.AssetCode })
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct()
            .ToList();

        List<AuditLog> auditLogs;
        if (assetReferences.Count > 0)
        {
            auditLogs = await _context.AuditLogs
                .AsNoTracking()
                .Where(log => !log.IsDeleted && assetReferences.Contains(log.EntityId))
                .OrderByDescending(log => log.CreatedAt)
                .ToListAsync(cancellationToken);
        }
        else
        {
            auditLogs = new List<AuditLog>();
        }

        var userLookup = await BuildUserLookupAsync(auditLogs, cancellationToken);

        var latestAuditByReference = auditLogs
            .GroupBy(log => log.EntityId)
            .ToDictionary(group => group.Key, group => group.First());

        var rows = assets.Select((asset, index) =>
        {
            latestAuditByReference.TryGetValue(asset.Id.ToString(), out var latestById);

            AuditLog? latestByCode = null;
            if (!string.IsNullOrWhiteSpace(asset.AssetCode))
            {
                latestAuditByReference.TryGetValue(asset.AssetCode, out latestByCode);
            }

            var latestLog = (latestById, latestByCode) switch
            {
                (not null, not null) => latestById.CreatedAt >= latestByCode.CreatedAt ? latestById : latestByCode,
                (not null, null) => latestById,
                (null, not null) => latestByCode,
                _ => null
            };

            User? actor = null;
            if (latestLog is not null && !string.IsNullOrWhiteSpace(latestLog.CreatedBy))
            {
                userLookup.TryGetValue(latestLog.CreatedBy, out actor);
            }

            return new ReportingAssetRowDto
            {
                Id = asset.Id,
                AssetId = asset.AssetCode ?? string.Empty,
                Swatch = ReportingQueryHelpers.GetColor(index),
                ImageClass = ReportingQueryHelpers.ResolveImageClass(asset.ProductName ?? "unknown"),
                Product = asset.ProductName ?? "Unknown Product",
                Status = asset.Status.HasValue ? ReportingQueryHelpers.FormatAssetStatus(asset.Status.Value) : "Unknown",
                CheckedBy = latestLog is null ? null : ReportingQueryHelpers.ResolveActorDisplay(actor, latestLog.CreatedBy),
                CheckedRole = latestLog is null ? null : ReportingQueryHelpers.ResolveRoleDisplay(actor),
                AssuraName = asset.AssignedFirstName is null
                    ? (asset.DivisionName ?? "N/A")
                    : $"{asset.AssignedFirstName} {asset.AssignedLastName}".Trim(),
                Serial = string.IsNullOrWhiteSpace(asset.SerialNumber) ? "--" : asset.SerialNumber,
                Warranty = string.IsNullOrWhiteSpace(asset.Warranty) ? "Unavailable" : asset.Warranty,
                EndOfLife = asset.AssetDate != default ? asset.AssetDate.AddYears(5).ToString("yyyy") : "N/A",
                CodeNumber = asset.AssetTag ?? asset.AssetCode ?? string.Empty
            };
        }).ToList();

        return new ReportingAssetsPageDto
        {
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            Assets = rows
        };
    }

    private async Task<Dictionary<string, User>> BuildUserLookupAsync(IEnumerable<AuditLog> logs, CancellationToken cancellationToken)
    {
        var rawValues = logs
            .Select(l => l.CreatedBy)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var numericIds = rawValues
            .Where(value => int.TryParse(value, out _))
            .Select(value => int.Parse(value!))
            .ToList();

        var usernames = rawValues
            .Where(value => !int.TryParse(value, out _))
            .ToList();

        var users = await _context.Users
            .AsNoTracking()
            .Where(u => numericIds.Contains(u.Id) || usernames.Contains(u.Username))
            .ToListAsync(cancellationToken);

        var lookup = new Dictionary<string, User>(StringComparer.OrdinalIgnoreCase);

        foreach (var user in users)
        {
            lookup[user.Id.ToString()] = user;
            lookup[user.Username] = user;
        }

        return lookup;
    }
}
