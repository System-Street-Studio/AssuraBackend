using Assura.Domain.Common;

namespace Assura.Domain.Entities;

public class Supplier : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Website { get; set; }

    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    public ICollection<PurchasingOrder> PurchasingOrders { get; set; } = new List<PurchasingOrder>();
}
