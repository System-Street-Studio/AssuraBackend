namespace Assura.Application.Features.AssetRequests.DTOs;

public class AssetRequestDto 
{
    public int Id { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string AssetCategory { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedDate { get; set; }
    public string Department { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int? Quantity { get; set; }
    public string RequestType { get; set; } = string.Empty;
}