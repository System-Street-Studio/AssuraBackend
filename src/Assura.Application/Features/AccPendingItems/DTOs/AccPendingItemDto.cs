namespace Assura.Application.Features.AccPendingItems.DTOs;

public class AccPendingItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public string CurrentUser { get; set; } = string.Empty;
    public string RequestedByName { get; set; } = string.Empty;
    public string SpecialNote { get; set; } = string.Empty;
    public string ValueAtPurchasing { get; set; } = string.Empty;
    public string CurrentValue { get; set; } = string.Empty;
    public bool IsHighlighted { get; set; }
    public string? AssigneeName { get; set; }
    public int? BuyerId { get; set; }
    public string? BuyerName { get; set; }
    public decimal? SoldPrice { get; set; }
}
