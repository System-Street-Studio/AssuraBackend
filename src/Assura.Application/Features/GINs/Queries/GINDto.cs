namespace Assura.Application.Features.GINs.Queries;

public class GINDto
{
    public int Id { get; set; }
    public string GinNumber { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
    public string? Condition { get; set; }
    public string? Notes { get; set; }

    public int GRNId { get; set; }
    public string GrnNumber { get; set; } = string.Empty;

    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? AssignedUserName { get; set; }

    public DateTime CreatedAt { get; set; }
}
