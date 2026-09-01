using System.Text.Json;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.SystemAdmin.Commands;

/// <summary>
/// Directly creates a SystemAdmin user account (no division — SystemAdmin accounts aren't tied
/// to one), bypassing the public self-registration -> HR-assignment pipeline. Only Admin/SystemAdmin
/// can invoke this (enforced by the controller's [Authorize]). The equivalent HR path
/// (<see cref="CreateHrAccountCommand"/>) is deliberately separate — it issues system-generated
/// credentials instead of taking caller-supplied ones, since HR accounts are meant to be handed
/// off to someone who completes their own profile on first login.
/// </summary>
public record CreatePrivilegedUserCommand : IRequest<CreatePrivilegedUserResult>
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }

    public string? ActorName { get; init; }
    public string? IpAddress { get; init; }
}

public record CreatePrivilegedUserResult(bool Success, string? Error, int? UserId);

public class CreatePrivilegedUserCommandValidator : AbstractValidator<CreatePrivilegedUserCommand>
{
    public CreatePrivilegedUserCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.Password)
            .Must((command, password) =>
                !string.Equals(password, command.Username, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Password must not be the same as the username.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
    }
}

public class CreatePrivilegedUserCommandHandler : IRequestHandler<CreatePrivilegedUserCommand, CreatePrivilegedUserResult>
{
    private readonly IApplicationDbContext _context;

    public CreatePrivilegedUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CreatePrivilegedUserResult> Handle(CreatePrivilegedUserCommand request, CancellationToken cancellationToken)
    {
        var usernameOrEmailTaken = await _context.Users.AnyAsync(
            u => u.Username == request.Username || u.Email == request.Email, cancellationToken);
        if (usernameOrEmailTaken)
        {
            return new CreatePrivilegedUserResult(false, "Username or email already exists.", null);
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.SystemAdmin,
            DivisionId = null,
            EmploymentStatus = "Assigned",
            AssignedAt = DateTime.UtcNow,
            IsActive = true,
            IsLocked = false
        };

        _context.Users.Add(user);

        _context.AuditLogs.Add(new AuditLog
        {
            EntityName = "SystemAdmin",
            Action = "Created SystemAdmin User",
            CreatedBy = request.ActorName,
            IpAddress = request.IpAddress,
            NewValues = JsonSerializer.Serialize(new
            {
                username = request.Username,
                email = request.Email,
                role = UserRole.SystemAdmin.ToString()
            })
        });

        await _context.SaveChangesAsync(cancellationToken);

        _context.Notifications.Add(new Notification
        {
            Title = "Account Created",
            Message = "An account with the SystemAdmin role has been created for you by an administrator.",
            UserId = user.Id,
            Type = "Success"
        });
        await _context.SaveChangesAsync(cancellationToken);

        return new CreatePrivilegedUserResult(true, null, user.Id);
    }
}
