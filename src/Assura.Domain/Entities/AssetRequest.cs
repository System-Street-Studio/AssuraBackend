using Assura.Domain.Common; 
using Assura.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Assura.Domain.Entities;

public class AssetRequest : BaseEntity
{
    [Required]
    public string AssetName { get; set; } = string.Empty;

    public string AssetCategory { get; set; } = string.Empty;

    [Required]
    public string Priority { get; set; } = string.Empty; // High, Normal, Low

    public string? Description { get; set; } = string.Empty;

    public int? Quantity { get; set; }

    public string? Reason { get; set; } = string.Empty;


    // Enum 
    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    [Required]
    public string RequesterId { get; set; } = string.Empty;

    public string RequesterName { get; set; } = string.Empty;

    [Required]
    public string RequestType { get; set; } = string.Empty; // NewAsset, Transfer, Maintenance, Discard

    public DateTime SubmittedDate { get; set; } = DateTime.Now;

    public int? DivisionId { get; set; }
    public Division? Division { get; set; }

    public int? UserId { get; set; }
    public User? User { get; set; }

    public int? AssetId { get; set; }
    public Asset? Asset { get; set; }

    public ICollection<AssetAttachment> Attachments { get; set; } = new List<AssetAttachment>();    
}