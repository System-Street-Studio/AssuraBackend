using Assura.Application.Common.Interfaces;
using MediatR;

namespace Assura.Application.Features.Users.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, string?>
{
    private readonly IIdentifyServices _identifyServices;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(IIdentifyServices identifyServices, IEmailService emailService)
    {
        _identifyServices = identifyServices;
        _emailService = emailService;
    }

    public async Task<string?> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var token = await _identifyServices.GeneratePasswordResetTokenAsync(request.Email);
        
        if (token != null)
        {
            var subject = "Assura - Password Reset Request";
            var body = $@"
                <h3>Password Reset Request</h3>
                <p>Hello,</p>
                <p>You requested to reset your password. Please use the following token to complete the process:</p>
                <div style='padding: 15px; background-color: #f3f3f3; border-radius: 5px; font-weight: bold; font-size: 1.2rem; text-align: center;'>
                    {token}
                </div>
                <p>If you did not request this, please ignore this email.</p>
                <p>Thank you,<br/>Assura Team</p>";

            await _emailService.SendEmailAsync(request.Email, subject, body);
        }

        return token;
    }
}
