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
}
