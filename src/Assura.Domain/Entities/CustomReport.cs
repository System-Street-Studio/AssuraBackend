using Assura.Domain.Common;

namespace Assura.Domain.Entities;

public class CustomReport : BaseEntity
{
    public string ReportIdCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string Size { get; set; } = "0 KB";
    
    public bool IsScheduled { get; set; }
    public string? ScheduleFrequency { get; set; }
    public DateTime? NextRunDate { get; set; }
}
