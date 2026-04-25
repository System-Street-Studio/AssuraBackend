using Assura.Domain.Common;

namespace Assura.Domain.Entities;

/// <summary>
/// Accountant Discard Note entity — formal notes about discarded items.
/// </summary>
public class AccDiscardNote : BaseEntity
{
    public string AssetName { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Note { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Approved or Pending
    public string AssetType { get; set; } = string.Empty;
    public string CurrentUser { get; set; } = string.Empty;
    public TimeSpan Time { get; set; }
    public decimal ValueAtPurchasing { get; set; }
    public decimal CurrentValue { get; set; }
}
