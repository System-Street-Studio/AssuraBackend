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
}
