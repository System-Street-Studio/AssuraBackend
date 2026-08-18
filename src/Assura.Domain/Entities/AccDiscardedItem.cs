using Assura.Domain.Common;

namespace Assura.Domain.Entities;

/// <summary>
/// Accountant Discarded Item — items confirmed as discarded by accountant.
/// </summary>
public class AccDiscardedItem : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string AssetType { get; set; } = string.Empty;
    public string CurrentUser { get; set; } = string.Empty;
    public string? RequestedByName { get; set; }
    public string SpecialNote { get; set; } = string.Empty;
    public decimal ValueAtPurchasing { get; set; }
    public decimal CurrentValue { get; set; }
    public TimeSpan Time { get; set; }

    /// <summary>
    /// The Receipt the Accountant attached as proof of disposal when confirming this
    /// discard — required by ConfirmDiscardCommand, so every discarded item has one.
    /// </summary>
    public int? ReceiptId { get; set; }
}
