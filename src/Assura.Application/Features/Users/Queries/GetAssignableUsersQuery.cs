using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Users.Queries;

public record GetAssignableUsersQuery : IRequest<List<AssignableUserDto>>;
public class AssignableUserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int? DivisionId { get; set; }
    public string? DivisionName { get; set; }
}

public class GetAssignableUsersQueryHandler : IRequestHandler<GetAssignableUsersQuery, List<AssignableUserDto>>
{
    private readonly IApplicationDbContext _context;
    public GetAssignableUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AssignableUserDto>> Handle(GetAssignableUsersQuery request, CancellationToken cancellationToken)
    {
        return await _context.Users
            .AsNoTracking()
            .Include(u => u.Division)
            .Where(u => u.IsActive && u.Role == UserRole.Employee)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Select(u => new AssignableUserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                FullName = (u.FirstName + " " + u.LastName).Trim(),
                Email = u.Email,
                Department = u.Division != null ? u.Division.Name : "N/A",
                DivisionId = u.DivisionId,
                DivisionName = u.Division != null ? u.Division.Name : null
            })
            .ToListAsync(cancellationToken);
    }
}

