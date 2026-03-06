using Assura.Application.Common.Interfaces;
using Assura.Application.Common.Models;
using MediatR;

namespace Assura.Application.Features.Users.Commands.Login;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AuthResponse?>
{
    private readonly IIdentifyServices _identifyServices;

    public LoginUserCommandHandler(IIdentifyServices identifyServices)
    {
        _identifyServices = identifyServices;
    }

    public async Task<AuthResponse?> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        return await _identifyServices.AuthenticateAsync(request.Username, request.Password);
    }
}
