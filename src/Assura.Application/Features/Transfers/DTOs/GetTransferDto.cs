using System.ComponentModel.DataAnnotations;

namespace Assura.Application.Features.Transfers.DTOs;

public class TransferDto
{
    public int Id { get; set; }
    public string TransferNumber { get; set; } = string.Empty;
    public DateTime TransferDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    
    public string AssetTag { get; set; } = string.Empty;
    public string AssetCode { get; set; } = string.Empty;
    public int AssetId { get; set; }
    public string AssetStatus { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    
    public int? AssetRequestId { get; set; }
    
    public int? FromDivisionId { get; set; }
    public string FromDivisionName { get; set; } = string.Empty;
    public int? ToDivisionId { get; set; }
    public string ToDivisionName { get; set; } = string.Empty;
    
    public string TransferByName { get; set; } = string.Empty;  
    public int? TransferById { get; set; }   
    public int TargetUserId { get; set; }
    public string? TargetUserName { get; set; } = string.Empty;
    public int? CurrentHolderId { get; set; }
    public string? CurrentHolderName { get; set; }
    
    public string? Reason { get; set; }
    public string? TransferPeriod { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}