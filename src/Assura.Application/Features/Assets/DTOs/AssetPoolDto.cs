using System.Collections.Generic;

namespace Assura.Application.Features.Assets.DTOs;

public class AssetPoolDto
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string AssetCode { get; set; } = string.Empty;
    public string AssetTag { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int? AssignedUserId { get; set; }
    public string AssignedUserName { get; set; } = string.Empty;
    public int? DivisionId { get; set; }
    public string DivisionName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public Dictionary<string, string> Specifications { get; set; } = new();
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}

public class DivisionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class AssetPoolFilterResult
{
    public List<AssetPoolDto> Assets { get; set; } = new();
    public List<DivisionDto> Divisions { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
