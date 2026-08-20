using Assura.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Users.Commands.CompleteOnboarding;

/// <summary>
/// Lets a user with <see cref="Assura.Domain.Entities.User.RequiresOnboarding"/> set (e.g. an HR
/// account created with a system-generated username/password) claim their account on first login:
/// choose their own username, password, and fill in the name/email/phone that weren't collected
/// at creation.
/// </summary>
public record CompleteOnboardingCommand : IRequest<CompleteOnboardingResult>
{
    public int UserId { get; init; }
    public string NewUsername { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
}

public record CompleteOnboardingResult(bool Success, string? Error, string? Token);

public class CompleteOnboardingCommandValidator : AbstractValidator<CompleteOnboardingCommand>
{
    public CompleteOnboardingCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.NewUsername).NotEmpty().MaximumLength(50);
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public class CompleteOnboardingCommandHandler : IRequestHandler<CompleteOnboardingCommand, CompleteOnboardingResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentifyServices _identityService;

    public CompleteOnboardingCommandHandler(IApplicationDbContext context, IIdentifyServices identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<CompleteOnboardingResult> Handle(CompleteOnboardingCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null)
        {
            return new CompleteOnboardingResult(false, "User not found.", null);
        }

        if (!user.RequiresOnboarding)
        {
            return new CompleteOnboardingResult(false, "This account does not require onboarding.", null);
        }

        var usernameTaken = await _context.Users.AnyAsync(
            u => u.Id != user.Id && u.Username == request.NewUsername, cancellationToken);
        if (usernameTaken)
        {
            return new CompleteOnboardingResult(false, "That username is already taken.", null);
        }

        var emailTaken = await _context.Users.AnyAsync(
            u => u.Id != user.Id && u.Email == request.Email, cancellationToken);
        if (emailTaken)
        {
            return new CompleteOnboardingResult(false, "That email is already in use.", null);
        }

        user.Username = request.NewUsername;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.PhoneNumber = request.PhoneNumber;
        user.RequiresOnboarding = false;

        _context.AuditLogs.Add(new Assura.Domain.Entities.AuditLog
        {
            EntityName = "Users",
            EntityId = user.Id.ToString(),
            Action = "Completed Onboarding",
            CreatedBy = request.NewUsername
        });

        await _context.SaveChangesAsync(cancellationToken);

        var newToken = await _identityService.RegenerateTokenAsync(user.Id);
        return new CompleteOnboardingResult(true, null, newToken);
    }
}
