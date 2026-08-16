using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
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

        _context.Notifications.Add(new Notification
        {
            Title = user.IsLocked ? "Account Locked" : "Account Unlocked",
            Message = user.IsLocked
                ? "Your account was locked by an administrator. Contact them if you believe this is a mistake."
                : "Your account has been unlocked by an administrator. You can log in again.",
            UserId = user.Id,
            Type = user.IsLocked ? "Warning" : "Info"
        });

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
