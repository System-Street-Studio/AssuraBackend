using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Assura.Infrastructure.Identity;

public class IdentityService : IIdentifyServices
{
    private readonly IApplicationDbContext _context;

    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public IdentityService(IApplicationDbContext context, IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<bool> RegisterAsync(
        string username,
        string password,
        string email,
        string firstName,
        string lastName,
        string? phoneNumber = null,
        string? requestedRole = null,
        int? divisionId = null)
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Username = username,
            PasswordHash = passwordHash,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            RequestedRole = requestedRole,
            DivisionId = divisionId,
            // Role and DivisionID are assigned later by HR
            EmploymentStatus = "PendingAssignment",
            IsActive = true,
            IsLocked = false
        };

        _context.Users.Add(user);
        return await _context.SaveChangesAsync(default) > 0;
    }

    public async Task<bool> UserExistsAsync(string username, string email)
    {
        return await _context.Users.AnyAsync(u => u.Username == username || u.Email == email);
    }

    public async Task<Assura.Application.Common.Models.AuthResponse?> AuthenticateAsync(string username, string password)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username || u.Email == username);

        if (user == null)
        {
            return null;
        }

        if (user.IsLocked)
        {
            throw new UnauthorizedAccessException("Your account has been locked by the system administrator.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException(
                user.EmploymentStatus == "Rejected"
                    ? "Your registration was rejected by HR. Please contact HR for more information."
                    : "Your account is inactive. Please contact HR for more information.");
        }

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

        if (!isPasswordValid)
        {
            return null;
        }

        // Invalidate any previous session by assigning a fresh session identifier
        var sessionId = Guid.NewGuid().ToString();
        user.CurrentSessionId = sessionId;
        await _context.SaveChangesAsync(default);

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new Assura.Application.Common.Models.AuthResponse
        {
            Token = token,
            User = new Assura.Application.Common.Models.UserDto
            {
                Id = user.Id.ToString(),
                Email = user.Email,
                Name = $"{user.FirstName} {user.LastName}",
                Roles = new List<string> { user.Role?.ToString() ?? "Employee" }
            }
        };
    }

    public async Task LogoutAsync(int userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user != null)
        {
            user.CurrentSessionId = null;
            await _context.SaveChangesAsync(default);
        }
    }

    public async Task<string?> GeneratePasswordResetTokenAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return null;

        var token = Guid.NewGuid().ToString();
        user.PasswordResetToken = token;
        user.ResetTokenExpiryTime = DateTime.UtcNow.AddHours(1);

        await _context.SaveChangesAsync(default);
        return token;
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.PasswordResetToken == token);

        if (user == null || user.ResetTokenExpiryTime < DateTime.UtcNow)
            return false;

        if (user.IsLocked || !user.IsActive)
            return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.PasswordResetToken = null;
        user.ResetTokenExpiryTime = null;

        // Invalidate any existing session/refresh token so a device that was already
        // logged in (potentially an attacker's, if this reset is recovering a
        // compromised account) doesn't stay authenticated after the password changes.
        // Note: this must be a fresh value, not null - the JWT session-check in
        // Infrastructure/DependencyInjection.cs's OnTokenValidated only rejects a
        // stale token when CurrentSessionId is non-empty AND mismatches the token's
        // claim, so a null value would actually disable the check entirely.
        user.CurrentSessionId = Guid.NewGuid().ToString();
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;

        return await _context.SaveChangesAsync(default) > 0;
    }
}
