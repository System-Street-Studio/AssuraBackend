namespace Assura.Application.DTOs;

public class InformStoresDto
{
    public string ItemName { get; set; } = string.Empty;
    public string? Model { get; set; }
    public string? Warranty { get; set; }
    public int Quantity { get; set; }
    public DateTime PurchasedDate { get; set; }
    public decimal PurchasedPrice { get; set; }
    public int DivisionId { get; set; }
}
