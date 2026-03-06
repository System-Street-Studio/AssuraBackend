using Assura.Domain.Common;

namespace Assura.Domain.Entities;

public class AssetInforming : BaseEntity
{
    public string ItemName { get; set; } = string.Empty;
    public string? Model { get; set; }
    public string? Warranty { get; set; }
    public int Quantity { get; set; }
    public DateTime PurchasedDate { get; set; }
    public decimal PurchasedPrice { get; set; }
    public string Status { get; set; } = "Pending";

    public int DivisionId { get; set; }
    public Division Division { get; set; } = null!;
}
