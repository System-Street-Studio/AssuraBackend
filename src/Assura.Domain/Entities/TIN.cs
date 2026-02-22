using Assura.Domain.Common;

namespace Assura.Domain.Entities;

public class TIN : BaseEntity
{
    public string TinNumber { get; set; } = string.Empty;
    public DateTime TransferInDate { get; set; }
    public string? ReceivedBy { get; set; }

    public int AssetId { get; set; }
    public Asset Asset { get; set; } = null!;
}
