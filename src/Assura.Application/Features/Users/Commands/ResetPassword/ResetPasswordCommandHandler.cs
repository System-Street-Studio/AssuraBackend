using Assura.Application.Common.Interfaces;
using MediatR;

namespace Assura.Application.Features.Users.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly IIdentifyServices _identifyServices;

    public ResetPasswordCommandHandler(IIdentifyServices identifyServices)
    {
        _identifyServices = identifyServices;
    }

    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        return await _identifyServices.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
    }
}
