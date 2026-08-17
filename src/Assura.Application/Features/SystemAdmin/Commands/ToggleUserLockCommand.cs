using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.SystemAdmin.Commands;

public record ToggleUserLockCommand(int UserId, int CallerUserId) : IRequest<bool>;

public class ToggleUserLockCommandValidator : AbstractValidator<ToggleUserLockCommand>
{
    public ToggleUserLockCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0).WithMessage("User ID must be greater than 0.");
        RuleFor(x => x.CallerUserId).GreaterThan(0).WithMessage("Caller User ID must be greater than 0.");
    }
}

public class ToggleUserLockCommandHandler : IRequestHandler<ToggleUserLockCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ToggleUserLockCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ToggleUserLockCommand request, CancellationToken cancellationToken)
    {
        // Prevent self-targeting
        if (request.UserId == request.CallerUserId)
        {
            return false;
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null) return false;

        // Prevent locking the master system admin (hardcoded check)
        if (user.Username == "sysadmin") return false;

        // Prevent locking other Admin or SystemAdmin accounts
        if (user.Role == UserRole.Admin || user.Role == UserRole.SystemAdmin)
        {
            return false;
        }

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
