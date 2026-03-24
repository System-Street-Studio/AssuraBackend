namespace Assura.Application.DTOs;

public class SupplierDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Website { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; }
}
