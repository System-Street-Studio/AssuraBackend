using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.HR.Queries;

public record GetAssignedHrUsersQuery : IRequest<List<AssignedHrUserDto>>;

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
        return await _context.Users
            .AsNoTracking()
            .Include(u => u.Division)
            .Where(u => u.IsActive && u.Role != null)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Select(u => new AssignedHrUserDto
            {
                Id = u.Id,
                UserId = u.Username,
                Name = (u.FirstName + " " + u.LastName).Trim(),
                Role = u.Role.HasValue ? u.Role.Value.ToString() : "Unassigned",
                Division = u.Division != null ? u.Division.Name : "Unassigned",
                JoinedDate = u.CreatedAt.ToString("yyyy-MM-dd"),
                JobTitle = string.IsNullOrWhiteSpace(u.JobTitle) ? "N/A" : u.JobTitle!,
                Status = u.EmploymentStatus
            })
            .ToListAsync(cancellationToken);
    }
}
