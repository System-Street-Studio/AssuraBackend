using Assura.Domain.Common;

namespace Assura.Domain.Entities;

public class RepairingFirm : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }

    public ICollection<Maintenance> MaintenanceRecords { get; set; } = new List<Maintenance>();
}
