using Assura.Domain.Common;

namespace Assura.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? ModelNumber { get; set; }
    public string? Manufacturer { get; set; }
    public string? Description { get; set; }

    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
