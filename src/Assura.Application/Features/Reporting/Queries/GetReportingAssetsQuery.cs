using Assura.Application.Common.Interfaces;
using Assura.Application.Features.Reporting.DTOs;
using Assura.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Reporting.Queries;

public record GetReportingAssetsQuery : IRequest<ReportingAssetsPageDto>;

public class GetReportingAssetsQueryHandler : IRequestHandler<GetReportingAssetsQuery, ReportingAssetsPageDto>
{
    private readonly IApplicationDbContext _context;

    public GetReportingAssetsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReportingAssetsPageDto> Handle(GetReportingAssetsQuery request, CancellationToken cancellationToken)
    {
        var assets = await _context.Assets
            .AsNoTracking()
            .Where(a => !a.IsDeleted)
            .Include(a => a.Product)
            .Include(a => a.Division)
            .Include(a => a.AssignedUser)
            .OrderByDescending(a => a.CreatedAt)
            .Take(200)
            .ToListAsync(cancellationToken);

        var assetReferences = assets
            .SelectMany(asset => new[] { asset.Id.ToString(), asset.AssetCode })
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToList();

        var auditLogs = await _context.AuditLogs
            .AsNoTracking()
            .Where(log => !log.IsDeleted && assetReferences.Contains(log.EntityId))
            .OrderByDescending(log => log.CreatedAt)
            .ToListAsync(cancellationToken);

        var userLookup = await BuildUserLookupAsync(auditLogs, cancellationToken);

        var latestAuditByReference = auditLogs
            .GroupBy(log => log.EntityId)
            .ToDictionary(group => group.Key, group => group.First());

        var rows = assets.Select((asset, index) =>
        {
            latestAuditByReference.TryGetValue(asset.Id.ToString(), out var latestById);
            latestAuditByReference.TryGetValue(asset.AssetCode, out var latestByCode);
            var latestLog = latestById?.CreatedAt >= latestByCode?.CreatedAt ? latestById : latestByCode ?? latestById;

            User? actor = null;
            if (latestLog is not null && !string.IsNullOrWhiteSpace(latestLog.CreatedBy))
            {
                userLookup.TryGetValue(latestLog.CreatedBy, out actor);
            }

            return new ReportingAssetRowDto
            {
                Id = asset.Id,
                AssetId = asset.AssetCode,
                Selected = index < 2,
                Swatch = ReportingQueryHelpers.GetColor(index),
                ImageClass = ReportingQueryHelpers.ResolveImageClass(asset.Product.Name),
                Product = asset.Product.Name,
                Status = ReportingQueryHelpers.FormatAssetStatus(asset.Status),
                CheckedBy = latestLog is null ? null : ReportingQueryHelpers.ResolveActorDisplay(actor, latestLog.CreatedBy),
                CheckedRole = latestLog is null ? null : ReportingQueryHelpers.ResolveRoleDisplay(actor),
                AssuraName = asset.AssignedUser is null
                    ? asset.Division.Name
                    : $"{asset.AssignedUser.FirstName} {asset.AssignedUser.LastName}".Trim(),
                Serial = string.IsNullOrWhiteSpace(asset.SerialNumber) ? "--" : asset.SerialNumber,
                Warranty = string.IsNullOrWhiteSpace(asset.Warranty) ? "Unavailable" : asset.Warranty,
                EndOfLife = asset.AssetDate.AddYears(5).ToString("yyyy"),
                CodeNumber = asset.AssetTag ?? asset.AssetCode
            };
        }).ToList();

        return new ReportingAssetsPageDto
        {
            SelectedCount = rows.Count(r => r.Selected),
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
            .Select(int.Parse)
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
