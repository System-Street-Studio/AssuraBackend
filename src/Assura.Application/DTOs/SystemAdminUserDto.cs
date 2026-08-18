namespace Assura.Application.DTOs;

public class SystemAdminUserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Role { get; set; }
    public bool IsLocked { get; set; }
    public bool IsActive { get; set; }
    public string EmploymentStatus { get; set; } = string.Empty;
}
