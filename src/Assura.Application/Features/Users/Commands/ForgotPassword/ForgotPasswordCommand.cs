using MediatR;

namespace Assura.Application.Features.Users.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<string?>;
