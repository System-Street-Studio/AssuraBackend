using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.HR.Queries;

public record GetHrUserByIdQuery(int UserId) : IRequest<HrUserDetailDto?>;

public class UserDivisionRoleDto
{
    public int DivisionId { get; set; }
    public string DivisionName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class HrUserDetailDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? RequestedRole { get; set; }
    public string? AssignedRole { get; set; }
    public string EmploymentStatus { get; set; } = string.Empty;
    public int? DivisionId { get; set; }
    public string Division { get; set; } = "Unassigned";
    public string? JobTitle { get; set; }
    public string JoinedDate { get; set; } = string.Empty;
    public string? AssignedAt { get; set; }
    public List<UserDivisionRoleDto> Assignments { get; set; } = new();
}

public class GetHrUserByIdQueryHandler : IRequestHandler<GetHrUserByIdQuery, HrUserDetailDto?>
{
    private readonly IApplicationDbContext _context;

    public GetHrUserByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HrUserDetailDto?> Handle(GetHrUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Division)
            .Include(u => u.DivisionRoles)
                .ThenInclude(dr => dr.Division)
            .Where(u => u.Id == request.UserId && u.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null) return null;

        return new HrUserDetailDto
        {
            Id = user.Id,
            UserId = user.Username,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Name = (user.FirstName + " " + user.LastName).Trim(),
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            RequestedRole = user.RequestedRole,
            AssignedRole = user.Role.HasValue ? user.Role.Value.ToString() : null,
            EmploymentStatus = user.EmploymentStatus,
            DivisionId = user.DivisionId,
            Division = user.Division != null ? user.Division.Name : "Unassigned",
            JobTitle = user.JobTitle,
            JoinedDate = user.CreatedAt.ToString("yyyy-MM-dd"),
            AssignedAt = user.AssignedAt.HasValue ? user.AssignedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
            Assignments = user.DivisionRoles.Select(dr => new UserDivisionRoleDto
            {
                DivisionId = dr.DivisionId,
                DivisionName = dr.Division.Name,
                Role = dr.Role.ToString()
            }).ToList()
        };
    }
}
