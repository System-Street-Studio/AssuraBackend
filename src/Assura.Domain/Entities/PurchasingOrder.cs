using Assura.Domain.Common;

namespace Assura.Domain.Entities;

public class PurchasingOrder : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Status { get; set; }

    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;

    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    public ICollection<GRN> GRNs { get; set; } = new List<GRN>();
    public ICollection<DiscountInfo> Discounts { get; set; } = new List<DiscountInfo>();
    public ICollection<PurchasingOrderItem> Items { get; set; } = new List<PurchasingOrderItem>();
}
