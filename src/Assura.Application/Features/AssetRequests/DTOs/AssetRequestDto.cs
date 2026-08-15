namespace Assura.Application.Features.AssetRequests.DTOs;

public class AssetRequestDto 
{
    public int Id { get; set; }
    public string RequesterId { get; set; } = string.Empty;
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
    public string? ProcessedByName { get; set; }
    public string? ProcessorRemarks { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? RejectionReason { get; set; }
    public List<AttachmentDto> Attachments { get; set; } = new();
}

public class AttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileType { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public DateTime UploadedDate { get; set; }
}