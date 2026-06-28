using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.SystemAdmin.Commands;

public record ResetUserPasswordCommand(int UserId) : IRequest<bool>;

public class ResetUserPasswordCommandHandler : IRequestHandler<ResetUserPasswordCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ResetUserPasswordCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null) return false;

        // Prevent resetting the master system admin password if needed (optional safety check)
        if (user.Username == "sysadmin") return false;

        // Reset the password to the default standard password "Password@123"
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password@123");

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
