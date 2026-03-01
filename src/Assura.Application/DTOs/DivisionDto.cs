namespace Assura.Application.DTOs;

/// <summary>
/// Represents a division within the organization.
/// </summary>
public class DivisionDto
{
    /// <summary>
    /// The unique identifier of the division.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The name of the division.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// An optional description of the division.
    /// </summary>
    public string? Description { get; set; }
}
