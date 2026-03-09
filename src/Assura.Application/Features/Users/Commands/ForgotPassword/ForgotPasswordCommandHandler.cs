using Assura.Application.Common.Interfaces;
using MediatR;

namespace Assura.Application.Features.Users.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, string?>
{
    private readonly IIdentifyServices _identifyServices;

    public ForgotPasswordCommandHandler(IIdentifyServices identifyServices)
    {
        _identifyServices = identifyServices;
    }

    public async Task<string?> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        // In a real application, you would generate the token and send an email here.
        // For now, we return the token to the controller which might return it for testing,
        // or we'll just return a success message.
        return await _identifyServices.GeneratePasswordResetTokenAsync(request.Email);
    }
}
