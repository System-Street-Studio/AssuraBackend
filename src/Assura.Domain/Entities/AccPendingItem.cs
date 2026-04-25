using Assura.Domain.Common;
using Assura.Domain.Enums;

namespace Assura.Domain.Entities;

/// <summary>
/// Accountant Pending Item — tracks asset disposal requests through accountant approval workflow.
/// </summary>
public class AccPendingItem : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty; // e.g. "Pending (Waiting for Confirmation)", "Approved", etc.
    public AccPendingCategory Category { get; set; } = AccPendingCategory.Pending;
    public TimeSpan Time { get; set; }
    public string AssetType { get; set; } = string.Empty;
    public string CurrentUser { get; set; } = string.Empty;
    public string SpecialNote { get; set; } = string.Empty;
    public decimal ValueAtPurchasing { get; set; }
    public decimal CurrentValue { get; set; }
    public bool IsHighlighted { get; set; } = false;
}
