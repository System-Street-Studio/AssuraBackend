namespace Assura.Application.Features.GRNs.Queries;

public class GRNDto
{
    public int Id { get; set; }
    public string GrnNumber { get; set; } = string.Empty;
    public DateTime ReceivedDate { get; set; }
    public string? ReceivedBy { get; set; }
    public string? Notes { get; set; }

    public int PurchasingOrderId { get; set; }
    public string PurchasingOrderNumber { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;

    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
