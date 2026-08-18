using Assura.Domain.Common;
using Assura.Domain.Enums;

namespace Assura.Domain.Entities;

public class Maintenance : BaseEntity
{
    public string MaintenanceNumber { get; set; } = string.Empty;
    public MaintenanceType Type { get; set; }
    public DateTime MaintenanceDate { get; set; }
    public string? Description { get; set; }
    public decimal Cost { get; set; }
    public string? Status { get; set; }

    // Workflow fields
    public string? Priority { get; set; }
    public string? IssueType { get; set; }
    public string? Notes { get; set; }

    // Requester (Employee who requested)
    public int? RequestedByUserId { get; set; }
    public User? RequestedByUser { get; set; }

    // Manager who approved
    public int? ApprovedByUserId { get; set; }
    public User? ApprovedByUser { get; set; }

    // Storekeeper who processed
    public int? StorekeeperUserId { get; set; }
    public User? StorekeeperUser { get; set; }

    // Temporary replacement asset from store
    public int? ReplacementAssetId { get; set; }
    public Asset? ReplacementAsset { get; set; }

    // Link to the originating request. Polymorphic: may reference either
    // Requests.Id or AssetRequests.Id depending on which workflow raised this
    // maintenance record — callers that need to resolve it (see
    // InformMaintenanceStakeholdersCommand, EscalateToProcurementCommand) look it
    // up in both tables. No navigation property/FK constraint on purpose, since a
    // single FK can't target two different tables.
    public int? OriginalRequestId { get; set; }

    // Audit timestamps
    public DateTime? ApprovedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? EscalatedToProcurementAt { get; set; }

    // Primary asset
    public int AssetId { get; set; }
    public Asset Asset { get; set; } = null!;

    // Repairing firm
    public int? RepairingFirmId { get; set; }
    public RepairingFirm? RepairingFirm { get; set; }
}
