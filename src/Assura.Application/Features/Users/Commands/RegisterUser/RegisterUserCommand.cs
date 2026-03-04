using System.ComponentModel.DataAnnotations;
using Assura.Application.Common.Interfaces;
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
        // register the user
         return await _identifyServices.RegisterAsync(
            request.Username, request.Password, request.Email, request.FirstName, request.LastName);
    }
}