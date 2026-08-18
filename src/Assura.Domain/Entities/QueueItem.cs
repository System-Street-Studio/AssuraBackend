using Assura.Domain.Common;
using Assura.Domain.Enums;

namespace Assura.Domain.Entities;

/// <summary>
/// Superintendent Overview queue item — tracks discard requests through review.
/// </summary>
public class QueueItem : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public QueueItemStatus Status { get; set; } = QueueItemStatus.Unread;
    public TimeSpan Time { get; set; }
    public string AssetType { get; set; } = string.Empty;
    public string SpecialNote { get; set; } = string.Empty;
    public string? ReviewNote { get; set; }

    /// <summary>
    /// The employee who originally requested this asset be discarded (from the
    /// source AssetRequest), distinct from whoever later reviews/approves it.
    /// </summary>
    public string? RequestedById { get; set; }
    public string? RequestedByName { get; set; }
}
