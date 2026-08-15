using Assura.Domain.Common;

namespace Assura.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DepreciationRate { get; set; } = 10.0m;

    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
