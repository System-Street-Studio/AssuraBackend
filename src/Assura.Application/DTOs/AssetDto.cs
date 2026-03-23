using Assura.Domain.Enums;

namespace Assura.Application.DTOs;

public class AssetDto
{
    public int Id { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string? AssetTag { get; set; }
    public DateTime AssetDate { get; set; }
    public AssetStatus Status { get; set; }
    public string? SerialNumber { get; set; }
    public decimal PurchaseValue { get; set; }
    public string? Warranty { get; set; }
    public string? Notes { get; set; }

    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    public int DivisionId { get; set; }
    public string DivisionName { get; set; } = string.Empty;

    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;

    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;

    public int? AssignedUserId { get; set; }
    public string? AssignedUserName { get; set; }
}

public class AssetCreateDto
{
    public string AssetCode { get; set; } = string.Empty;
    public string? AssetTag { get; set; }
    public DateTime AssetDate { get; set; }
    public AssetStatus Status { get; set; }
    public string? SerialNumber { get; set; }
    public decimal PurchaseValue { get; set; }
    public string? Warranty { get; set; }
    public string? Notes { get; set; }

    public int CategoryId { get; set; }
    public int DivisionId { get; set; }
    public int ProductId { get; set; }
    public int SupplierId { get; set; }
    public int? AssignedUserId { get; set; }
}

public class AssetUpdateDto : AssetCreateDto
{
    public int Id { get; set; }
}
