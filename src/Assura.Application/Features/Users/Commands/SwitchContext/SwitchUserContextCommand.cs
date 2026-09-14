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

        // Apply new context to user
        user.Role = parsedRole;
        if (request.DivisionId.HasValue && request.DivisionId.Value > 0)
        {
            user.DivisionId = request.DivisionId.Value;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new SwitchUserContextResult(true, null, token, user.DivisionId, user.Role.Value.ToString());
    }
}
