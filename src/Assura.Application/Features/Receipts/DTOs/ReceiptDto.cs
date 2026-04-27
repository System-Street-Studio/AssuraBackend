namespace Assura.Application.Features.Receipts.DTOs;

public class ReceiptDto
{
    public string Id { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Amount { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class CreateReceiptDto
{
    public string AssetName { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Amount { get; set; } = string.Empty;
}
