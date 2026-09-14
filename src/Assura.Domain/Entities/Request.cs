using Assura.Domain.Common;
using Assura.Domain.Enums;

namespace Assura.Domain.Entities;

public class Request : BaseEntity
{
    public string RequestNumber { get; set; } = string.Empty;
    public RequestType Type { get; set; }
    public PriorityType Priority { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Remarks { get; set; }
    public string? Specifications { get; set; }
    public string? SpecialNote { get; set; }
    public bool RequiresDivisionHeadApproval { get; set; } = true;

    public int? DivisionHeadReviewerId { get; set; }
    public DateTime? DivisionHeadReviewedAt { get; set; }

    public int? StorekeeperProcessorId { get; set; }
    public DateTime? StorekeeperProcessedAt { get; set; }

    public DateTime? TemporarilyAssignedAt { get; set; }
    public DateTime? PickupConfirmedAt { get; set; }

    public int RequesterId { get; set; }
    public User Requester { get; set; } = null!;

    public int? DivisionId { get; set; }
    public Division? Division { get; set; }

    public int? AssetId { get; set; }
    public Asset? Asset { get; set; }

    // Unified request properties (consolidated from AssetRequest)
    public string? AssetName { get; set; }
    public string? AssetCategory { get; set; }
    public int? Quantity { get; set; }
    public string? Reason { get; set; }
    public string? RejectionReason { get; set; }
    public string? ProcessedByName { get; set; }
    public string? ProcessorRemarks { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? SubmittedDate { get; set; }

    // Attachments for the request
    public ICollection<AssetAttachment> Attachments { get; set; } = new List<AssetAttachment>();

    // Set when Procurement raises a Purchasing Order against this request (see
    // CreatePurchasingOrderCommand). Lets asset registration for that PO (CreateAssetCommand)
    // find its way back to whichever request(s) were actually waiting on it, instead of the
    // request going PendingProcurement -> Approved and then never being resolved further.
    public int? PurchasingOrderId { get; set; }
    public PurchasingOrder? PurchasingOrder { get; set; }
}
