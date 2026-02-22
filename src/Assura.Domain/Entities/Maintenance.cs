using Assura.Domain.Common;
using Assura.Domain.Enums;

namespace Assura.Domain.Entities;

public class Maintenance : BaseEntity
{
    public string MaintenanceNumber { get; set; } = string.Empty;
    public MaintenanceType Type { get; set; }
    public DateTime MaintenanceDate { get; set; }
    public string? Description { get; set; }
    public decimal Cost { get; set; }
    public string? Status { get; set; }

    public int AssetId { get; set; }
    public Asset Asset { get; set; } = null!;

    public int? RepairingFirmId { get; set; }
    public RepairingFirm? RepairingFirm { get; set; }
}
