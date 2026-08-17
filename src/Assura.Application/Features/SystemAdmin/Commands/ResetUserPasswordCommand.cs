using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Assura.Application.Features.SystemAdmin.Commands;

public record ResetUserPasswordCommand(int UserId, int CallerUserId) : IRequest<ResetUserPasswordResult>;

public record ResetUserPasswordResult(bool Success, string? TemporaryPassword);

public class ResetUserPasswordCommandValidator : AbstractValidator<ResetUserPasswordCommand>
{
    public ResetUserPasswordCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0).WithMessage("User ID must be greater than 0.");
        RuleFor(x => x.CallerUserId).GreaterThan(0).WithMessage("Caller User ID must be greater than 0.");
    }
}

public class ResetUserPasswordCommandHandler : IRequestHandler<ResetUserPasswordCommand, ResetUserPasswordResult>
{
    private readonly IApplicationDbContext _context;

    public ResetUserPasswordCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ResetUserPasswordResult> Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        // Prevent self-targeting
        if (request.UserId == request.CallerUserId)
        {
            return new ResetUserPasswordResult(false, null);
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null) return new ResetUserPasswordResult(false, null);

        // Prevent resetting the master system admin password (hardcoded check)
        if (user.Username == "sysadmin") return new ResetUserPasswordResult(false, null);

        // Prevent resetting other Admin or SystemAdmin accounts
        if (user.Role == UserRole.Admin || user.Role == UserRole.SystemAdmin)
        {
            return new ResetUserPasswordResult(false, null);
        }

        var temporaryPassword = GenerateTemporaryPassword();
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(temporaryPassword);

        _context.Notifications.Add(new Notification
        {
            Title = "Password Reset by Administrator",
            Message = "Your password was reset by an administrator. Contact them directly for your new temporary password, and change it as soon as you log in.",
            UserId = user.Id,
            Type = "Warning"
        });

        await _context.SaveChangesAsync(cancellationToken);
        return new ResetUserPasswordResult(true, temporaryPassword);
    }

    /// <summary>
    /// Generates a random one-time password, guaranteed to contain an uppercase letter, a
    /// lowercase letter, a digit, and a special character. Replaces the previous hardcoded
    /// "Password@123" default, which every reset produced identically and predictably.
    /// </summary>
    private static string GenerateTemporaryPassword()
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnpqrstuvwxyz";
        const string digits = "23456789";
        const string special = "!@#$%^&*";
        const string all = upper + lower + digits + special;

        var chars = new char[12];
        chars[0] = upper[RandomNumberGenerator.GetInt32(upper.Length)];
        chars[1] = lower[RandomNumberGenerator.GetInt32(lower.Length)];
        chars[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
        chars[3] = special[RandomNumberGenerator.GetInt32(special.Length)];
        for (var i = 4; i < chars.Length; i++)
        {
            chars[i] = all[RandomNumberGenerator.GetInt32(all.Length)];
        }

        for (var i = chars.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars);
    }
}
