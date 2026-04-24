using Assura.Domain.Enums;

namespace Assura.Application.Features.Assets.DTOs;

public class AssetWithAssignmentDto
{
    public int Id { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string AssetCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string DivisionName { get; set; } = string.Empty;
    public AssetStatus Status { get; set; }
    public string? AssignedUserName { get; set; }
    public string? AssignedUserEmail { get; set; }
    public int? AssignedUserId { get; set; }
    public string? SerialNumber { get; set; }
    public decimal PurchaseValue { get; set; }
    public DateTime AssetDate { get; set; }
    public string? Notes { get; set; }
    public string? QrCode { get; set; }
}
