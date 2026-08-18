using Assura.Domain.Common;
using Assura.Domain.Enums;

namespace Assura.Domain.Entities;

public class DiscardedNote : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public DiscardNoteStatus Status { get; set; } = DiscardNoteStatus.Pending;
    public TimeSpan Time { get; set; }
    public string AssetType { get; set; } = string.Empty;
    public string SpecialNote { get; set; } = string.Empty;

    // Employee who originally raised the discard request, so Superintendent/Admin
    // can see who is responsible for it alongside the originating division.
    public int? RequestedByUserId { get; set; }
    public string? RequestedByName { get; set; }

    // The Asset this note is disposing of, carried over from the source AssetRequest.
    // Lets the accountant's final discard confirmation flip the real Asset record to
    // Discarded instead of only updating this note's own status.
    public int? AssetId { get; set; }

    // The QueueItem created alongside this note (for the Superintendent Overview
    // dashboard), so completing this note can carry the link through to the
    // AccPendingItem it spawns — ConfirmDiscardCommand needs that link to flip the
    // QueueItem to Discarded once the accountant confirms.
    public int? QueueItemId { get; set; }
}
