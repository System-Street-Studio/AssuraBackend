using Assura.Domain.Common;

namespace Assura.Domain.Entities;

public class Notification : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public string? Type { get; set; } // Info, Warning, Error
    public string? ReferenceId { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
