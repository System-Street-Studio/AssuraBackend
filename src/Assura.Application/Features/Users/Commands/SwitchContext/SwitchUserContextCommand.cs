using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assura.Application.Features.Users.Commands.SwitchContext;

public record SwitchUserContextCommand : IRequest<SwitchUserContextResult>
{
    public int UserId { get; init; }
    public int? DivisionId { get; init; }
    public string Role { get; init; } = string.Empty;
}

public record SwitchUserContextResult(bool Success, string? Error, string? Token, int? DivisionId, string? Role);

public class SwitchUserContextCommandHandler : IRequestHandler<SwitchUserContextCommand, SwitchUserContextResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public SwitchUserContextCommandHandler(IApplicationDbContext context, IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<SwitchUserContextResult> Handle(SwitchUserContextCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.DivisionRoles)
            .ThenInclude(dr => dr.Division)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return new SwitchUserContextResult(false, "User not found.", null, null, null);
        }

        if (!Enum.TryParse<UserRole>(request.Role, true, out var parsedRole))
        {
            return new SwitchUserContextResult(false, "Invalid role specified.", null, null, null);
        }

        // Validate that user has access to this role/division
        bool isAuthorized = false;

        if (user.Role == UserRole.SystemAdmin || user.Role == UserRole.Admin)
        {
            isAuthorized = true;
        }
        else if (user.DivisionRoles != null && user.DivisionRoles.Any(dr =>
            dr.Role == parsedRole &&
            (!request.DivisionId.HasValue || dr.DivisionId == request.DivisionId.Value)))
        {
            isAuthorized = true;
        }
        else if (user.Role == parsedRole && (!request.DivisionId.HasValue || user.DivisionId == request.DivisionId.Value))
        {
            isAuthorized = true;
        }
        else if (parsedRole == UserRole.Employee && ((user.DivisionRoles != null && user.DivisionRoles.Any(dr => !request.DivisionId.HasValue || dr.DivisionId == request.DivisionId.Value)) || user.DivisionId == request.DivisionId))
        {
            isAuthorized = true;
        }

        if (!isAuthorized)
        {
            return new SwitchUserContextResult(false, "You are not authorized for this role and division.", null, null, null);
        }

        // Ensure user's permanent role is preserved in UserDivisionRoles if not already present
        if (user.Role.HasValue && user.DivisionId.HasValue && user.DivisionId.Value > 0)
        {
            if (user.DivisionRoles != null && !user.DivisionRoles.Any(dr => dr.Role == user.Role.Value && dr.DivisionId == user.DivisionId.Value))
            {
                user.DivisionRoles.Add(new Domain.Entities.UserDivisionRole
                {
                    UserId = user.Id,
                    DivisionId = user.DivisionId.Value,
                    Role = user.Role.Value,
                    JobTitle = user.JobTitle,
                    AssignedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        var targetDivisionId = request.DivisionId.HasValue && request.DivisionId.Value > 0
            ? request.DivisionId.Value
            : user.DivisionId;

        // Generate token with active role and division context without overwriting user.Role in the database
        var token = _jwtTokenGenerator.GenerateToken(user, request.Role, targetDivisionId);

        return new SwitchUserContextResult(true, null, token, targetDivisionId, request.Role);
    }
}
