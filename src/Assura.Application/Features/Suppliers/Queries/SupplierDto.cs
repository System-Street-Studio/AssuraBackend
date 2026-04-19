namespace Assura.Application.Features.Suppliers.Queries;

public class SupplierDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Website { get; set; }
    public string? Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; }
    
    // UI specific fields if needed
    public string? ContactNumber => Phone;
    public string? Url => Website;
    public string? DateRegistered => CreatedAt.ToString("yyyy MMM dd");
}
