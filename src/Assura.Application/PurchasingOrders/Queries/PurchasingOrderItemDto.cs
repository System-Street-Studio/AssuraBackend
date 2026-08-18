namespace Assura.Application.PurchasingOrders.Queries;

public class PurchasingOrderItemDto
{
    public int Id { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? Model { get; set; }
    public string? Warranty { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
    public decimal Discount { get; set; }
    public decimal DiscountedPrice { get; set; }
    public decimal VatPercentage { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalPrice { get; set; }
    public string? SpecialNote { get; set; }
}
