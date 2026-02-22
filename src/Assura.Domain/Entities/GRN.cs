using Assura.Domain.Common;

namespace Assura.Domain.Entities;

public class GRN : BaseEntity
{
    public string GrnNumber { get; set; } = string.Empty;
    public DateTime ReceivedDate { get; set; }
    public string? ReceivedBy { get; set; }
    public string? Notes { get; set; }

    public int PurchasingOrderId { get; set; }
    public PurchasingOrder PurchasingOrder { get; set; } = null!;

    public int AssetId { get; set; }
    public Asset Asset { get; set; } = null!;
}
