using Assura.Domain.Common;
using Assura.Domain.Enums;

namespace Assura.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public bool IsLocked { get; set; } = false;
    public bool IsActive { get; set; } = true;

    public int DivisionId { get; set; }
    public Division Division { get; set; } = null!;

    public UserRole Role { get; set; }
    
    public ICollection<Asset> AssignedAssets { get; set; } = new List<Asset>();
    public ICollection<Request> Requests { get; set; } = new List<Request>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
