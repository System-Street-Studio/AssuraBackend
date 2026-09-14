using Assura.Application.Common.Interfaces;

using MediatR;

using Microsoft.EntityFrameworkCore;



namespace Assura.Application.Features.Users.Queries;



public record GetUserProfileQuery(int UserId) : IRequest<UserProfileDto?>;



public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto?>

{

    private readonly IApplicationDbContext _context;



    public GetUserProfileQueryHandler(IApplicationDbContext context)

    {

        _context = context;

    }



    public async Task<UserProfileDto?> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)

    {

        Console.WriteLine($"[DEBUG] GetUserProfileQueryHandler: Handling request for UserId: {request.UserId}");

        

        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Division)
            .Include(u => u.DivisionRoles)
                .ThenInclude(dr => dr.Division)
            .Where(u => u.Id == request.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        Console.WriteLine($"[DEBUG] GetUserProfileQueryHandler: Result found: {(user != null ? "Yes" : "No")}");

        if (user == null) return null;

        var workspaces = user.DivisionRoles.Select(dr => new UserWorkspaceDto
        {
            DivisionId = dr.DivisionId,
            DivisionName = dr.Division != null ? dr.Division.Name : string.Empty,
            Role = dr.Role.ToString(),
            JobTitle = dr.JobTitle
        }).ToList();

        if (user.Role.HasValue && !workspaces.Any(w => w.Role.Equals(user.Role.Value.ToString(), System.StringComparison.OrdinalIgnoreCase) && w.DivisionId == user.DivisionId))
        {
            workspaces.Insert(0, new UserWorkspaceDto
            {
                DivisionId = user.DivisionId,
                DivisionName = user.Division != null ? user.Division.Name : string.Empty,
                Role = user.Role.Value.ToString(),
                JobTitle = user.JobTitle
            });
        }

        var distinctDivisions = workspaces
            .Where(w => w.DivisionId.HasValue && w.DivisionId > 0)
            .Select(w => new { Id = w.DivisionId!.Value, Name = w.DivisionName })
            .Distinct()
            .ToList();

        foreach (var div in distinctDivisions)
        {
            if (!workspaces.Any(w => w.Role.Equals("Employee", System.StringComparison.OrdinalIgnoreCase) && w.DivisionId == div.Id))
            {
                workspaces.Add(new UserWorkspaceDto
                {
                    DivisionId = div.Id,
                    DivisionName = div.Name,
                    Role = "Employee",
                    JobTitle = "Employee"
                });
            }
        }

        return new UserProfileDto
        {
            Id = user.Id,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.HasValue ? user.Role.Value.ToString() : "User",
            DivisionId = user.DivisionId,
            DivisionName = user.Division != null ? user.Division.Name : null,
            PhoneNumber = user.PhoneNumber,
            Workspaces = workspaces
        };

    }

}

