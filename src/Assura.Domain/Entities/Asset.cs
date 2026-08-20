using Assura.Domain.Common;
using Assura.Domain.Enums;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assura.Domain.Entities;

public class Asset : BaseEntity
{
    public string AssetCode { get; set; } = string.Empty;
    public string? AssetTag { get; set; }
    public DateTime AssetDate { get; set; }
    public AssetStatus Status { get; set; }
    public string? SerialNumber { get; set; }
    public decimal PurchaseValue { get; set; }
    public string? Warranty { get; set; }
    public string? Notes { get; set; }
    public string? QrCode { get; set; }

    [NotMapped]
    public AssetSpecifications? Specifications { get; set; }

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    public int? DivisionId { get; set; }
    public Division? Division { get; set; }

    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public int? AssignedUserId { get; set; }
    public User? AssignedUser { get; set; }

    public int? ReservedForUserId { get; set; }
    public DateTime? ReservedUntilUtc { get; set; }
    public int? ReservedByRequestId { get; set; }

    // Set when this asset is registered against a Purchasing Order (see AssetCreateDto /
    // AssetFormComponent's PO auto-fill). Lets UpdatePurchasingOrderStatusCommand find the
    // asset that was just bought for a PO and hand it to whichever request was waiting on
    // that PO, instead of PendingProcurement -> Approved being the end of the line. A bare
    // id like ReservedByRequestId above, not a full FK relationship — kept simple since
    // nothing needs to navigate from Asset to PurchasingOrder in code today.
    public int? PurchasingOrderId { get; set; }

    public DateTime? LastVerifiedAt { get; set; }
    public int? LastVerifiedByUserId { get; set; }
    public User? LastVerifiedByUser { get; set; }

    [InverseProperty("Asset")]
    public ICollection<Maintenance> MaintenanceRecords { get; set; } = new List<Maintenance>();
    public ICollection<Transfer> Transfers { get; set; } = new List<Transfer>();
    public ICollection<AssetRequest> AssetRequests { get; set; } = new List<AssetRequest>();
}
