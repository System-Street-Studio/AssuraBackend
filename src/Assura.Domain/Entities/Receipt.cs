using Assura.Domain.Common;
using Assura.Domain.Enums;

namespace Assura.Domain.Entities;

/// <summary>
/// Receipt entity for accountant receipt management.
/// </summary>
public class Receipt : BaseEntity
{
    public string AssetName { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Amount { get; set; } = string.Empty;
    public ReceiptStatus Status { get; set; } = ReceiptStatus.Pending;
}
