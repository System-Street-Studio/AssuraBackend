using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.HR.Queries;

public record GetHrOverviewQuery : IRequest<HrOverviewDto>;

public class HrOverviewDto
{
    public List<HrStatDto> Stats { get; set; } = [];
    public List<HrDivisionCountDto> UsersByDivision { get; set; } = [];
}

public class HrStatDto
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class HrDivisionCountDto
{
    public string Division { get; set; } = string.Empty;
    public int Users { get; set; }
}

public class GetHrOverviewQueryHandler : IRequestHandler<GetHrOverviewQuery, HrOverviewDto>
{
    private readonly IApplicationDbContext _context;

    public GetHrOverviewQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HrOverviewDto> Handle(GetHrOverviewQuery request, CancellationToken cancellationToken)
    {
        var totalUsers = await _context.Users.CountAsync(u => u.IsActive, cancellationToken);
        var pendingUsers = await _context.Users.CountAsync(u => u.IsActive && u.Role == null, cancellationToken);
        var assignedUsers = await _context.Users.CountAsync(u => u.IsActive && u.Role != null, cancellationToken);
        var hrUsers = await _context.Users.CountAsync(u => u.IsActive && u.Role == UserRole.HR, cancellationToken);
        var totalDivisions = await _context.Divisions.CountAsync(cancellationToken);

        var usersByDivision = await _context.Users
            .AsNoTracking()
            .Include(u => u.Division)
            .Where(u => u.IsActive && u.Role != null)
            .GroupBy(u => u.Division != null ? u.Division.Name : "Unassigned")
            .Select(g => new HrDivisionCountDto
            {
                Division = g.Key,
                Users = g.Count()
            })
            .OrderByDescending(x => x.Users)
            .ThenBy(x => x.Division)
            .ToListAsync(cancellationToken);

        return new HrOverviewDto
        {
            Stats =
            [
                new HrStatDto { Label = "Total Users", Value = totalUsers },
                new HrStatDto { Label = "Pending Users", Value = pendingUsers },
                new HrStatDto { Label = "Assigned Users", Value = assignedUsers },
                new HrStatDto { Label = "HR Team", Value = hrUsers },
                new HrStatDto { Label = "Divisions", Value = totalDivisions }
            ],
            UsersByDivision = usersByDivision
        };
    }
}
