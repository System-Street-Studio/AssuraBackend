using Assura.Domain.Common; // BaseEntity එක තියෙන තැන
using Assura.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Assura.Domain.Entities;

public class AssetRequest 
{
    
    
   [Key]
    public int Id { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string AssetCategory { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Attachments { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    public string RequesterId { get; set; } = string.Empty;
    public string RequesterName { get; set; } = string.Empty;
    public string RequestType { get; set; } = string.Empty;
    public DateTime SubmittedDate { get; set; } 

}