using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.SystemAdmin.Queries;

public record GetSystemUsersQuery() : IRequest<List<SystemAdminUserDto>>;

public class GetSystemUsersQueryHandler : IRequestHandler<GetSystemUsersQuery, List<SystemAdminUserDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSystemUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SystemAdminUserDto>> Handle(GetSystemUsersQuery request, CancellationToken cancellationToken)
    {
        return await _context.Users
            .Select(u => new SystemAdminUserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role.ToString(),
                IsLocked = u.IsLocked,
                IsActive = u.IsActive,
                EmploymentStatus = u.EmploymentStatus
            })
            .OrderBy(u => u.Username)
            .ToListAsync(cancellationToken);
    }
}
