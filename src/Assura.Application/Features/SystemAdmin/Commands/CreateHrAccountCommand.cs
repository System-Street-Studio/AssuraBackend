using System.Security.Cryptography;
using System.Text.Json;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.SystemAdmin.Commands;

/// <summary>
/// Creates an HR account with a system-generated username and temporary password — the caller
/// (Admin/SystemAdmin) supplies nothing but gets the credentials back once, to hand off to the
/// HR employee out of band. The account is locked to Role=HR and the "Human Resource" division,
/// and is flagged <see cref="User.RequiresOnboarding"/> so the owner is forced to set their own
/// password and fill in their name/email/phone the first time they log in.
/// </summary>
public record CreateHrAccountCommand : IRequest<CreateHrAccountResult>
{
    public string? ActorName { get; init; }
    public string? IpAddress { get; init; }
}

public record CreateHrAccountResult(bool Success, string? Error, string? Username, string? TemporaryPassword);

public class CreateHrAccountCommandHandler : IRequestHandler<CreateHrAccountCommand, CreateHrAccountResult>
{
    private readonly IApplicationDbContext _context;

    public CreateHrAccountCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CreateHrAccountResult> Handle(CreateHrAccountCommand request, CancellationToken cancellationToken)
    {
        var hrDivision = await _context.Divisions
            .FirstOrDefaultAsync(d => d.Name == "Human Resource", cancellationToken);
        if (hrDivision == null)
        {
            return new CreateHrAccountResult(false, "Human Resource division is not configured.", null, null);
        }

        var username = await GenerateUniqueUsernameAsync(cancellationToken);
        var temporaryPassword = GenerateTemporaryPassword();

        var user = new User
        {
            Username = username,
            // Placeholder — unique (derived from the generated username) and clearly not a real
            // address, since the HR employee supplies their real email during onboarding.
            Email = $"{username}@pending.assura.local",
            FirstName = string.Empty,
            LastName = string.Empty,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(temporaryPassword),
            Role = UserRole.HR,
            DivisionId = hrDivision.Id,
            EmploymentStatus = "Assigned",
            AssignedAt = DateTime.UtcNow,
            IsActive = true,
            IsLocked = false,
            RequiresOnboarding = true
        };

        _context.Users.Add(user);

        _context.AuditLogs.Add(new AuditLog
        {
            EntityName = "SystemAdmin",
            Action = "Created HR Account (auto-generated credentials)",
            CreatedBy = request.ActorName,
            IpAddress = request.IpAddress,
            NewValues = JsonSerializer.Serialize(new { username, divisionId = hrDivision.Id })
        });

        await _context.SaveChangesAsync(cancellationToken);

        return new CreateHrAccountResult(true, null, username, temporaryPassword);
    }

    private async Task<string> GenerateUniqueUsernameAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            var candidate = $"hr{RandomNumberGenerator.GetInt32(1000, 999999)}";
            if (!await _context.Users.AnyAsync(u => u.Username == candidate, cancellationToken))
            {
                return candidate;
            }
        }

        // Astronomically unlikely after 20 collisions in the "hr1000".."hr999999" space, but
        // fall back to a value that's unique by construction rather than looping forever.
        return $"hr{Guid.NewGuid():N}"[..12];
    }

    /// <summary>Mirrors <c>ResetUserPasswordCommand.GenerateTemporaryPassword</c> — guarantees an
    /// uppercase letter, a lowercase letter, a digit, and a special character.</summary>
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
