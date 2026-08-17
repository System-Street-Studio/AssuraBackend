using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.HR.Queries;

public record GetRejectedHrUsersQuery(string? Search = null) : IRequest<List<RejectedHrUserDto>>;

public class RejectedHrUserDto
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

public class GetRejectedHrUsersQueryHandler : IRequestHandler<GetRejectedHrUsersQuery, List<RejectedHrUserDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRejectedHrUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<RejectedHrUserDto>> Handle(GetRejectedHrUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Users
            .AsNoTracking()
            .Include(u => u.Division)
            .Where(u => !u.IsActive && u.EmploymentStatus == "Rejected")
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(u =>
                u.Username.ToLower().Contains(search) ||
                u.FirstName.ToLower().Contains(search) ||
                u.LastName.ToLower().Contains(search) ||
                u.Email.ToLower().Contains(search) ||
                (u.RequestedRole != null && u.RequestedRole.ToLower().Contains(search)));
        }

        return await query
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new RejectedHrUserDto
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
