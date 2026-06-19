namespace Assura.Application.Features.Maintenances.Queries;

public class MaintenanceDto
{
    public int Id { get; set; }
    public string MaintenanceNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime MaintenanceDate { get; set; }
    public string? Description { get; set; }
    public decimal Cost { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? IssueType { get; set; }
    public string? Notes { get; set; }

    // Asset info
    public int AssetId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string? AssetCode { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }

    // Requester info
    public int? RequestedByUserId { get; set; }
    public string? RequestedByName { get; set; }
    public string? RequesterDivision { get; set; }

    // Approver info
    public int? ApprovedByUserId { get; set; }
    public string? ApprovedByName { get; set; }

    // Storekeeper info
    public int? StorekeeperUserId { get; set; }
    public string? StorekeeperName { get; set; }

    // Replacement asset info
    public int? ReplacementAssetId { get; set; }
    public string? ReplacementAssetCode { get; set; }
    public string? ReplacementAssetName { get; set; }

    // Repairing firm
    public int? RepairingFirmId { get; set; }
    public string? RepairingFirmName { get; set; }

    // Original request link
    public int? OriginalRequestId { get; set; }

    // Timestamps
    public DateTime? ApprovedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? EscalatedToProcurementAt { get; set; }
}

public class MaintenanceStatsDto
{
    public int Total { get; set; }
    public int PendingApproval { get; set; }
    public int Approved { get; set; }
    public int InProgress { get; set; }
    public int TempAssigned { get; set; }
    public int SentForRepair { get; set; }
    public int EscalatedToProcurement { get; set; }
    public int Completed { get; set; }
    public int Rejected { get; set; }
}

public class SimilarAssetDto
{
    public int Id { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal PurchaseValue { get; set; }
}
