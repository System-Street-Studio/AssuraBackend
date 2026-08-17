namespace Assura.Application.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ModelNumber { get; set; }
    public string? Manufacturer { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}

public class ProductCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? ModelNumber { get; set; }
    public string? Manufacturer { get; set; }
    public string? Description { get; set; }
}

public class ProductUpdateDto : ProductCreateDto
{
    public int Id { get; set; }
}
