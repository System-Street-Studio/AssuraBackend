namespace Assura.Application.PurchasingOrders.Queries;

public class AssetRequestDto
{
    public int Id { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string DivisionName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Specifications { get; set; }
    public string? SpecialNote { get; set; }
    public string? Type { get; set; }
    public string? Description { get; set; }
    public int? AssetId { get; set; }
    public string? AssetName { get; set; }
}
