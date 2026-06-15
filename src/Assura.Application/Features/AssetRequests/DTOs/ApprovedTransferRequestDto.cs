namespace Assura.Application.Features.AssetRequests.DTOs;

public class ApprovedTransferRequestDto
{
    public int Id { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public string RequesterId { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string AssetCategory { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string RequestType { get; set; } = string.Empty;
    public DateTime SubmittedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string Priority { get; set; } = string.Empty;
    public int? Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? DivisionId { get; set; }
    public string DivisionName { get; set; } = string.Empty;
    public int? TargetUserId { get; set; }
    public string TargetUserName { get; set; } = string.Empty;
}
