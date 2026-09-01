using System.ComponentModel.DataAnnotations;
using Assura.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace Assura.Application.Features.Users.Commands.RegisterUser;

public record RegisterUserCommand : IRequest<RegisterUserResult>
{
    [Required]
    public string Username { get; init; } = string.Empty; // Manually added by the user during registration

    [Required]
    public string Password { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string FirstName { get; init; } = string.Empty;

    [Required]
    public string LastName { get; init; } = string.Empty;

    public string? PhoneNumber { get; init; }

    /// <summary>Informational only — the role the registrant would like, shown to HR as a hint.
    /// Never the actual <see cref="Assura.Domain.Entities.User.Role"/>, which stays unset until
    /// HR/SystemAdmin assigns it.</summary>
    public string? RequestedRole { get; init; }
}

public record RegisterUserResult(bool Success, string? Error);

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Password)
            .Must((command, password) =>
                !string.Equals(password, command.Username, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Password must not be the same as the username.");
    }
}

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly IIdentifyServices _identifyServices;
    public RegisterUserCommandHandler(IIdentifyServices identifyServices)
    {
        _identifyServices = identifyServices;
    }
    public async Task<RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Check for specific conflicts (username, email, or password already taken)
        var conflict = await _identifyServices.CheckUserConflictAsync(request.Username, request.Email, request.Password);
        if (conflict != null)
        {
            return new RegisterUserResult(false, conflict);
        }

        // Register the user with Role/Division left unassigned — a self-registered account can
        // never set its own division (or role); only HR/SystemAdmin assignment can, later.
        // DivisionId is deliberately not sourced from the request here, unlike RequestedRole.
        var registered = await _identifyServices.RegisterAsync(
            request.Username,
            request.Password,
            request.Email,
            request.FirstName,
            request.LastName,
            request.PhoneNumber,
            request.RequestedRole,
            divisionId: null);

        return registered
            ? new RegisterUserResult(true, null)
            : new RegisterUserResult(false, "Registration failed. Please try again.");
    }
}
