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
            var resetLink = $"http://localhost:4200/auth/reset-password?token={token}&email={request.Email}";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px;'>
                    <h2 style='color: #003366; text-align: center;'>Password Reset Request</h2>
                    <p>Hello,</p>
                    <p>You requested to reset your password for your Assura account. Click the button below to set a new password:</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{resetLink}' style='background-color: #003366; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold; font-size: 16px;'>Reset Password</a>
                    </div>
                    <p>If the button doesn't work, you can copy and paste the following link into your browser:</p>
                    <p style='word-break: break-all; color: #666; font-size: 12px;'>{resetLink}</p>
                    <p>Alternatively, you can manually enter this token in the app:</p>
                    <div style='padding: 10px; background-color: #f3f3f3; border-radius: 5px; font-weight: bold; font-size: 1.1rem; text-align: center; border: 1px dashed #ccc;'>
                        {token}
                    </div>
                    <p>If you did not request this, please ignore this email.</p>
                    <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;' />
                    <p style='font-size: 12px; color: #888; text-align: center;'>Thank you,<br/>Assura Team</p>
                </div>";

            await _emailService.SendEmailAsync(request.Email, subject, body);
        }

        return token;
    }
}
