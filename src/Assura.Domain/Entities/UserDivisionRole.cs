using Assura.Domain.Common;
using Assura.Domain.Enums;

namespace Assura.Domain.Entities;

public class UserDivisionRole : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int DivisionId { get; set; }
    public Division Division { get; set; } = null!;

    public UserRole Role { get; set; }
    
    public string? JobTitle { get; set; }
    public string? Notes { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}
