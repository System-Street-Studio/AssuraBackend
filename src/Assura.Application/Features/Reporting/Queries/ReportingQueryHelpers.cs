using Assura.Domain.Entities;
using Assura.Domain.Enums;

namespace Assura.Application.Features.Reporting.Queries;

internal static class ReportingQueryHelpers
{
    internal static readonly string[] Palette =
    [
        "#0f766e",
        "#f59e0b",
        "#2563eb",
        "#7c3aed",
        "#dc2626",
        "#0891b2"
    ];

    internal static string GetColor(int index) => Palette[index % Palette.Length];

    internal static decimal ToPercent(decimal value, decimal total)
    {
        if (total <= 0)
        {
            return 0;
        }

        return Math.Round((value / total) * 100m, 2);
    }

    internal static string FormatAssetStatus(AssetStatus status) => status switch
    {
        AssetStatus.InUse => "Active",
        AssetStatus.InStore => "In Store",
        AssetStatus.UnderMaintenance => "Maintenance",
        AssetStatus.Discarded => "Discarded",
        AssetStatus.Transferred => "Transferred",
        AssetStatus.Lost => "Lost",
        _ => status.ToString()
    };

    internal static string ClassifyLogStatus(AuditLog log)
    {
        var text = $"{log.Action} {log.EntityName}".ToLowerInvariant();

        if (text.Contains("fail") || text.Contains("error"))
        {
            return "Failed";
        }

        if (text.Contains("delete") || text.Contains("discard") || text.Contains("reject") || text.Contains("exception"))
        {
            return "Flagged";
        }

        if (text.Contains("create") || text.Contains("update") || text.Contains("issue") || text.Contains("generate") || text.Contains("export") || text.Contains("verify") || text.Contains("complete"))
        {
            return "Completed";
        }

        return "Active";
    }

    internal static string ResolveModule(string entityName)
    {
        if (string.IsNullOrWhiteSpace(entityName))
        {
            return "General";
        }

        return entityName.Trim().ToLowerInvariant() switch
        {
            "asset" => "Assets",
            "assets" => "Assets",
            "request" => "Requests",
            "requests" => "Requests",
            "report" => "Reports",
            "reports" => "Reports",
            "export" => "Exports",
            "audit" => "Audit",
            _ => entityName
        };
    }

    internal static string BuildLogDetail(AuditLog log)
    {
        if (!string.IsNullOrWhiteSpace(log.NewValues) && !string.IsNullOrWhiteSpace(log.OldValues))
        {
            return $"Changed {log.EntityName} {log.EntityId} from previous values to the latest snapshot.";
        }

        if (!string.IsNullOrWhiteSpace(log.NewValues))
        {
            return $"Created or refreshed {log.EntityName} {log.EntityId} with new values.";
        }

        if (!string.IsNullOrWhiteSpace(log.OldValues))
        {
            return $"Captured previous values for {log.EntityName} {log.EntityId} before the action.";
        }

        return $"{log.Action} action was recorded for {log.EntityName} {log.EntityId}.";
    }

    internal static string ResolveImageClass(string productName)
    {
        var name = productName.ToLowerInvariant();

        if (name.Contains("phone") || name.Contains("iphone"))
        {
            return "phone";
        }

        if (name.Contains("tablet") || name.Contains("ipad") || name.Contains("surface"))
        {
            return "tablet";
        }

        if (name.Contains("monitor") || name.Contains("display"))
        {
            return "monitor";
        }

        return "laptop";
    }

    internal static string ResolveActorDisplay(User? user, string? rawCreatedBy)
    {
        if (user is null)
        {
            return string.IsNullOrWhiteSpace(rawCreatedBy) ? "System" : rawCreatedBy;
        }

        var fullName = $"{user.FirstName} {user.LastName}".Trim();
        return string.IsNullOrWhiteSpace(fullName) ? user.Username : fullName;
    }

    internal static string ResolveRoleDisplay(User? user)
    {
        return user?.Role?.ToString() ?? "System";
    }
}
