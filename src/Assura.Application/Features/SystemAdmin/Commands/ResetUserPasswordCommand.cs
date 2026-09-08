using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Assura.Application.Features.SystemAdmin.Commands;

public record ResetUserPasswordCommand(int UserId, int CallerUserId) : IRequest<ResetUserPasswordResult>;

public record ResetUserPasswordResult(bool Success, bool EmailSent);

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
    private readonly IEmailService _emailService;

    public ResetUserPasswordCommandHandler(IApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<ResetUserPasswordResult> Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        // Prevent self-targeting
        if (request.UserId == request.CallerUserId)
        {
            return new ResetUserPasswordResult(false, false);
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null) return new ResetUserPasswordResult(false, false);

        // Prevent resetting the master system admin password (hardcoded check)
        if (user.Username == "sysadmin") return new ResetUserPasswordResult(false, false);

        // Prevent resetting other Admin or SystemAdmin accounts
        if (user.Role == UserRole.Admin || user.Role == UserRole.SystemAdmin)
        {
            return new ResetUserPasswordResult(false, false);
        }

        var temporaryPassword = GenerateTemporaryPassword();
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(temporaryPassword);

        // Invalidate any existing session/refresh token so a user who is being locked
        // out (e.g. a compromised or departing account) doesn't stay authenticated on
        // an already-logged-in device after an administrator forces this reset. Must be
        // a fresh value, not null - see IdentityService.ResetPasswordAsync for why.
        user.CurrentSessionId = Guid.NewGuid().ToString();
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;

        _context.Notifications.Add(new Notification
        {
            Title = "Password Reset by Administrator",
            Message = "Your password was reset by an administrator. Check your email for the new temporary password, and change it as soon as you log in.",
            UserId = user.Id,
            Type = "Warning"
        });

        await _context.SaveChangesAsync(cancellationToken);

        // Send email with temporary password
        var emailSent = false;
        try
        {
            var subject = "Assura - Password Reset by Administrator";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px;'>
                    <h2 style='color: #003366; text-align: center;'>Password Reset Notice</h2>
                    <p>Hello {user.Username},</p>
                    <p>Your password has been reset by a System Administrator. Please use the following temporary password to log in:</p>
                    <div style='padding: 15px; background-color: #f3f3f3; border-radius: 5px; font-weight: bold; font-size: 1.2rem; text-align: center; border: 1px solid #003366; margin: 20px 0;'>
                        {temporaryPassword}
                    </div>
                    <p style='color: #d32f2f; font-weight: bold;'>⚠️ Important: Please change this password immediately after logging in.</p>
                    <p>For security reasons, your existing session has been invalidated. You will need to log in again with this temporary password.</p>
                    <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;' />
                    <p style='font-size: 12px; color: #888; text-align: center;'>Thank you,<br/>Assura System Administrator</p>
                </div>";

            await _emailService.SendEmailAsync(user.Email, subject, body);
            emailSent = true;
        }
        catch (Exception)
        {
            // Email sending failed, but password reset was successful
            // Don't fail the entire operation
            emailSent = false;
        }

        return new ResetUserPasswordResult(true, emailSent);
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
