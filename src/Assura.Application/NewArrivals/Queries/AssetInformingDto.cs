namespace Assura.Application.NewArrivals.Queries;

public class AssetInformingDto
{
    public int Id { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? Model { get; set; }
    public string? Warranty { get; set; }
    public int Quantity { get; set; }
    public DateTime PurchasedDate { get; set; }
    public decimal PurchasedPrice { get; set; }
    public string Status { get; set; } = "Pending";
    public int DivisionId { get; set; }
    public string DivisionName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
