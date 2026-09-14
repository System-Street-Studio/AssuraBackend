using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.HR.Queries;

public record GetAssignedHrUsersQuery(
    string? Search = null,
    string? Division = null,
    string? Role = null) : IRequest<List<AssignedHrUserDto>>;

public class AssignedHrUserRoleDto
{
    public int DivisionId { get; set; }
    public string DivisionName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class AssignedHrUserDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Division { get; set; } = "Unassigned";
    public string JoinedDate { get; set; } = string.Empty;
    public string JobTitle { get; set; } = "N/A";
    public string Status { get; set; } = string.Empty;
    public List<AssignedHrUserRoleDto> Assignments { get; set; } = new();
}

public class GetAssignedHrUsersQueryHandler : IRequestHandler<GetAssignedHrUsersQuery, List<AssignedHrUserDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAssignedHrUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AssignedHrUserDto>> Handle(GetAssignedHrUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Users
            .AsNoTracking()
            .Include(u => u.Division)
            .Include(u => u.DivisionRoles)
                .ThenInclude(dr => dr.Division)
            .Where(u => u.IsActive && (u.Role != null || u.DivisionRoles.Any()))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(u =>
                u.Username.ToLower().Contains(search) ||
                u.FirstName.ToLower().Contains(search) ||
                u.LastName.ToLower().Contains(search) ||
                (u.JobTitle != null && u.JobTitle.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.Division))
        {
            var division = request.Division.Trim().ToLower();
            query = query.Where(u =>
                (u.Division != null && u.Division.Name.ToLower() == division) ||
                u.DivisionRoles.Any(dr => dr.Division.Name.ToLower() == division));
        }

        if (!string.IsNullOrWhiteSpace(request.Role) &&
            Enum.TryParse<Assura.Domain.Enums.UserRole>(request.Role, true, out var parsedRole))
        {
            query = query.Where(u => u.Role == parsedRole || u.DivisionRoles.Any(dr => dr.Role == parsedRole));
        }

        var users = await query
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync(cancellationToken);

        return users.Select(u =>
        {
            var assignments = u.DivisionRoles.Select(dr => new AssignedHrUserRoleDto
            {
                DivisionId = dr.DivisionId,
                DivisionName = dr.Division?.Name ?? "Unassigned",
                Role = dr.Role.ToString()
            }).ToList();

            if (assignments.Count == 0 && u.Role.HasValue)
            {
                assignments.Add(new AssignedHrUserRoleDto
                {
                    DivisionId = u.DivisionId ?? 0,
                    DivisionName = u.Division?.Name ?? "Unassigned",
                    Role = u.Role.Value.ToString()
                });
            }

            var rolesSummary = assignments.Count > 0
                ? string.Join(", ", assignments.Select(a => a.Role).Distinct())
                : (u.Role.HasValue ? u.Role.Value.ToString() : "Unassigned");

            var divisionsSummary = assignments.Count > 0
                ? string.Join(", ", assignments.Select(a => a.DivisionName).Distinct())
                : (u.Division != null ? u.Division.Name : "Unassigned");

            return new AssignedHrUserDto
            {
                Id = u.Id,
                UserId = u.Username,
                Name = (u.FirstName + " " + u.LastName).Trim(),
                Role = rolesSummary,
                Division = divisionsSummary,
                JoinedDate = u.CreatedAt.ToString("yyyy-MM-dd"),
                JobTitle = string.IsNullOrWhiteSpace(u.JobTitle) ? "N/A" : u.JobTitle!,
                Status = u.EmploymentStatus,
                Assignments = assignments
            };
        }).ToList();
    }
}
