using Assura.Domain.Common;

namespace Assura.Domain.Entities;

public class QRN : BaseEntity
{
    public string QrnNumber { get; set; } = string.Empty;
    public DateTime InspectionDate { get; set; }
    public string? InspectedBy { get; set; }
    public bool IsPassed { get; set; }
    public string? Remarks { get; set; }

    public int AssetId { get; set; }
    public Asset Asset { get; set; } = null!;
}
