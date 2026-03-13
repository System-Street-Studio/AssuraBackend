namespace Assura.Application.Features.Maintenances.Queries;

public class MaintenanceDto
{
    public int Id { get; set; }
    public string MaintenanceNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime MaintenanceDate { get; set; }
    public string? Description { get; set; }
    public decimal Cost { get; set; }
    public string? Status { get; set; }
    public int AssetId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public int? RepairingFirmId { get; set; }
    public string? RepairingFirmName { get; set; }
}
