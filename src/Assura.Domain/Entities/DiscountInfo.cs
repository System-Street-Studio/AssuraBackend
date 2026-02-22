using Assura.Domain.Common;

namespace Assura.Domain.Entities;

public class DiscountInfo : BaseEntity
{
    public string Description { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercentage { get; set; }

    public int PurchasingOrderId { get; set; }
    public PurchasingOrder PurchasingOrder { get; set; } = null!;
}
