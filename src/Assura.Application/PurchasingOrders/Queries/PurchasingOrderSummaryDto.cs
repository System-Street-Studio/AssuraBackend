namespace Assura.Application.PurchasingOrders.Queries;

public class PurchasingOrderSummaryDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public DateTime IssuedDate { get; set; }
    public int? DivisionId { get; set; }
    public string? DivisionName { get; set; }
    public string? Status { get; set; }
}
