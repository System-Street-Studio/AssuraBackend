using Assura.Domain.Common;

namespace Assura.Domain.Entities;

public class Division : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
