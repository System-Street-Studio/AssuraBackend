using System.Collections.Generic;
using Assura.Domain.Enums;

namespace Assura.Application.Features.Users.Queries;

public class UserWorkspaceDto
{
    public int? DivisionId { get; set; }
    public string? DivisionName { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
}

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
    public List<UserWorkspaceDto> Workspaces { get; set; } = new();
}
