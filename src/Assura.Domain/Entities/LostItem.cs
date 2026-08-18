using Assura.Domain.Common;
using Assura.Domain.Enums;

namespace Assura.Domain.Entities;

/// <summary>
/// Lost Item entity — tracks lost/missing assets.
/// </summary>
public class LostItem : BaseEntity
{
    public string AssetName { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string ReportedBy { get; set; } = string.Empty;
    public LostItemStatus Status { get; set; } = LostItemStatus.Reported;
    public string AssetType { get; set; } = string.Empty;
    public TimeSpan Time { get; set; }
    public decimal ValueAtPurchasing { get; set; }
    public decimal CurrentValue { get; set; }
    public string Description { get; set; } = string.Empty;
}
