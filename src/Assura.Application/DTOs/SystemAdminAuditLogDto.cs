namespace Assura.Application.DTOs;

public class SystemAdminAuditLogDto
{
    public int Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
}
