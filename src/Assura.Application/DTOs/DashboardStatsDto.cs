namespace Assura.Application.DTOs;

public class DashboardStatsDto
{
    public int TotalAssets { get; set; }
    public int TotalUsers { get; set; }
    public List<StatItemDto> AssetsByDivision { get; set; } = new();
    public List<StatItemDto> AssetsByStatus { get; set; } = new();
    public List<StatItemDto> AssetsByCategory { get; set; } = new();
}

public class StatItemDto
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}
