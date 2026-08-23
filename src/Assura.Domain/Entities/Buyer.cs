using Assura.Domain.Common;
using Assura.Domain.Enums;

namespace Assura.Domain.Entities;

/// <summary>
/// Buyer entity for the buyer management feature.
/// </summary>
public class Buyer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public BuyerStatus Status { get; set; } = BuyerStatus.Active;

    /// <summary>
    /// The AccDiscardedItem this buyer purchased, when the Superintendent records a buyer
    /// against a specific sold-off disposal rather than as a generic contact-directory entry.
    /// Null for a buyer that hasn't (yet) been matched to a sale.
    /// </summary>
    public int? AccDiscardedItemId { get; set; }
}
