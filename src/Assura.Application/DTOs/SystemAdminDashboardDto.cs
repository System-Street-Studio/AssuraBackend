namespace Assura.Application.DTOs;

public class SystemAdminDashboardDto
{
    public int TotalDepartments { get; set; }
    public int ActiveCategories { get; set; }
    public int RecentLogins { get; set; }
    public int ActiveSessions { get; set; }
    public int ErrorLogsCount { get; set; }
    public int AuditLogsCount { get; set; }
    public string SystemHealth { get; set; } = "Optimal";
}
