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

    /// <summary>
    /// The originating QueueItem, when this pending item was raised from a Superintendent
    /// discard-approval decision. Used to flip the QueueItem to Discarded once the
    /// accountant confirms the asset has actually been disposed.
    /// </summary>
    public int? QueueItemId { get; set; }

    /// <summary>
    /// The employee who originally requested this asset be discarded, carried over from
    /// the source AssetRequest/QueueItem. Distinct from CurrentUser, which records whoever
    /// most recently acted on (approved) the request.
    /// </summary>
    public string? RequestedById { get; set; }
    public string? RequestedByName { get; set; }

    /// <summary>
    /// The Asset being disposed of, carried over from the source DiscardedNote/QueueItem.
    /// Used by ConfirmDiscardCommand to flip the real Asset record to Discarded once the
    /// accountant confirms the physical disposal.
    /// </summary>
    public int? AssetId { get; set; }

    /// <summary>
    /// The Buyer the Superintendent assigned when approving/discarding the originating
    /// QueueItem — required at that step so the Accountant can see who is buying the
    /// asset before financially confirming the discard. Carried through to the Buyer's
    /// AccDiscardedItemId link once ConfirmDiscardCommand actually creates the discarded item.
    /// </summary>
    public int? BuyerId { get; set; }

    /// <summary>
    /// The sale/sold price specified by the Superintendent when approving the discard.
    /// Displayed to the Accountant in the confirmation queue and preserved on the discarded record.
    /// </summary>
    public decimal? SoldPrice { get; set; }
}
