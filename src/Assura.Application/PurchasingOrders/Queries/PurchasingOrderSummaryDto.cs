namespace Assura.Application.PurchasingOrders.Queries;

public class PurchasingOrderSummaryDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty; // Mapping from Division
    public DateTime IssuedDate { get; set; }
}
