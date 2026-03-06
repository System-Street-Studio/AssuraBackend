using Assura.Application.Common.Models;
using MediatR;

namespace Assura.Application.Features.Users.Commands.Login;

public record LoginUserCommand(string Username, string Password) : IRequest<AuthResponse?>;
