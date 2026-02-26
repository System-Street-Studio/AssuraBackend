using Assura.Domain.Common;

namespace Assura.Domain.Entities;

public class GIN : BaseEntity
{
    public string GinNumber { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
    public string? Condition { get; set; }
    public string? Notes { get; set; }

    public int AssetId { get; set; }
    public Asset Asset { get; set; } = null!;

    public int GRNId { get; set; }
    public GRN GRN { get; set; } = null!;
}
