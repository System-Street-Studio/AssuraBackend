using Assura.Domain.Enums;

namespace Assura.Application.Features.Users.Queries;

public class UserProfileDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string? DivisionName { get; set; }
    public int? DivisionId { get; set; }
    public string? PhoneNumber { get; set; }
}
