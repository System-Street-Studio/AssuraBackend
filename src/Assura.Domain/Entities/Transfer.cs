using Assura.Domain.Common;

namespace Assura.Domain.Entities;

public class Transfer : BaseEntity
{
    public string TransferNumber { get; set; } = string.Empty;
    public DateTime TransferDate { get; set; }
    public string? Reason { get; set; }

    public int AssetId { get; set; }
    public Asset Asset { get; set; } = null!;

    public int FromDivisionId { get; set; }
    public Division FromDivision { get; set; } = null!;

    public int ToDivisionId { get; set; }
    public Division ToDivision { get; set; } = null!;

    public int TransferById { get; set; }
    public User TransferBy { get; set; } = null!;
}
