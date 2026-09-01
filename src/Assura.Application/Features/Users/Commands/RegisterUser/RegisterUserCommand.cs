using System.ComponentModel.DataAnnotations;
using Assura.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace Assura.Application.Features.Users.Commands.RegisterUser;

public record RegisterUserCommand : IRequest<bool>
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

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, bool>
{
    private readonly IIdentifyServices _identifyServices;
    public RegisterUserCommandHandler(IIdentifyServices identifyServices)
    {
        _identifyServices = identifyServices;
    }
    public async Task<bool> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // check the user already exists
        if (await _identifyServices.UserExistsAsync(request.Username, request.Email))
        {
            return false;
        }
        // Register the user with Role/Division left unassigned — a self-registered account can
        // never set its own division (or role); only HR/SystemAdmin assignment can, later.
        // DivisionId is deliberately not sourced from the request here, unlike RequestedRole.
         return await _identifyServices.RegisterAsync(
            request.Username,
            request.Password,
            request.Email,
            request.FirstName,
            request.LastName,
            request.PhoneNumber,
            request.RequestedRole,
            divisionId: null);
    }
}
