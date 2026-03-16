using Assura.Domain.Common; 
using Assura.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Assura.Domain.Entities;

public class AssetRequest 
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string AssetName { get; set; } = string.Empty;

    public string AssetCategory { get; set; } = string.Empty;

    [Required]
    public string Priority { get; set; } = string.Empty; // High, Normal, Low

    public string? Description { get; set; } = string.Empty;

    public int? Quantity { get; set; }

    public string? Reason { get; set; } = string.Empty;

    public string? Attachments { get; set; }

    // Enum 
    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    [Required]
    public string RequesterId { get; set; } = string.Empty;

    public string RequesterName { get; set; } = string.Empty;

    
    [Required]
    public string RequestType { get; set; } = string.Empty; // NewAsset, Transfer, Maintenance, Discard

    public DateTime SubmittedDate { get; set; } = DateTime.Now;
}