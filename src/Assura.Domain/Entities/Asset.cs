using Assura.Domain.Common;
using Assura.Domain.Enums;

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

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public int DivisionId { get; set; }
    public Division Division { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;

    public int? AssignedUserId { get; set; }
    public User? AssignedUser { get; set; }

    public ICollection<Maintenance> MaintenanceRecords { get; set; } = new List<Maintenance>();
    public ICollection<Transfer> Transfers { get; set; } = new List<Transfer>();
    public ICollection<AssetRequest> AssetRequests { get; set; } = new List<AssetRequest>();
}
