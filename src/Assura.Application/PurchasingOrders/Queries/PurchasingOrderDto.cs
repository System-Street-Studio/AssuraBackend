namespace Assura.Application.PurchasingOrders.Queries;

public class PurchasingOrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Status { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public List<PurchasingOrderItemDto> Items { get; set; } = new();
}
