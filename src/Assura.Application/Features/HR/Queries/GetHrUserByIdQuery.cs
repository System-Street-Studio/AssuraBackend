using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.HR.Queries;

public record GetHrUserByIdQuery(int UserId) : IRequest<HrUserDetailDto?>;

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
        return await _context.Users
            .AsNoTracking()
            .Include(u => u.Division)
            .Where(u => u.Id == request.UserId && u.IsActive)
            .Select(u => new HrUserDetailDto
            {
                Id = u.Id,
                UserId = u.Username,
                Username = u.Username,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Name = (u.FirstName + " " + u.LastName).Trim(),
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                RequestedRole = u.RequestedRole,
                AssignedRole = u.Role.HasValue ? u.Role.Value.ToString() : null,
                EmploymentStatus = u.EmploymentStatus,
                DivisionId = u.DivisionId,
                Division = u.Division != null ? u.Division.Name : "Unassigned",
                JobTitle = u.JobTitle,
                JoinedDate = u.CreatedAt.ToString("yyyy-MM-dd"),
                AssignedAt = u.AssignedAt.HasValue ? u.AssignedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : null
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
