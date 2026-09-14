
namespace Assura.Domain.Entities;

public class AssetAttachment
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileType { get; set; } = string.Empty;
    public DateTime UploadedDate { get; set; }

    // Relationship with AssetRequest (legacy)
    public int? AssetRequestId { get; set; }
    public AssetRequest? AssetRequest { get; set; }

    // Relationship with Request (unified)
    public int? RequestId { get; set; }
    public Request? Request { get; set; }
}