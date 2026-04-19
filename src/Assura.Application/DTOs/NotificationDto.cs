using System;

namespace Assura.Application.DTOs;

public class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public string? Type { get; set; }
    public string? ReferenceId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Icon { get; set; } = "info";
}
