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
}
