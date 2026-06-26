using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.SystemAdmin.Commands;

public record ToggleUserLockCommand(int UserId) : IRequest<bool>;

public class ToggleUserLockCommandHandler : IRequestHandler<ToggleUserLockCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ToggleUserLockCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ToggleUserLockCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null) return false;

        // Prevent locking the master system admin if needed (optional safety check)
        if (user.Username == "sysadmin") return false;

        user.IsLocked = !user.IsLocked;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
