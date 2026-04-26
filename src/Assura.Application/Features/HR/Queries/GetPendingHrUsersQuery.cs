using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.HR.Queries;

public record GetPendingHrUsersQuery : IRequest<List<PendingHrUserDto>>;

public class PendingHrUserDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string JoinedDate { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = "Unassigned";
    public string RequestedRole { get; set; } = "Not specified";
    public string Phone { get; set; } = "N/A";
    public string Status { get; set; } = string.Empty;
}

public class GetPendingHrUsersQueryHandler : IRequestHandler<GetPendingHrUsersQuery, List<PendingHrUserDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPendingHrUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PendingHrUserDto>> Handle(GetPendingHrUsersQuery request, CancellationToken cancellationToken)
    {
        return await _context.Users
            .AsNoTracking()
            .Include(u => u.Division)
            .Where(u => u.IsActive && u.Role == null)
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new PendingHrUserDto
            {
                Id = u.Id,
                UserId = u.Username,
                Name = (u.FirstName + " " + u.LastName).Trim(),
                JoinedDate = u.CreatedAt.ToString("yyyy-MM-dd"),
                Email = u.Email,
                Department = u.Division != null ? u.Division.Name : "Unassigned",
                RequestedRole = string.IsNullOrWhiteSpace(u.RequestedRole) ? "Not specified" : u.RequestedRole!,
                Phone = string.IsNullOrWhiteSpace(u.PhoneNumber) ? "N/A" : u.PhoneNumber!,
                Status = u.EmploymentStatus
            })
            .ToListAsync(cancellationToken);
    }
}
