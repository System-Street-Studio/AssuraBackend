using Assura.Application.Features.Users.Commands.RegisterUser;
using Assura.Application.Features.Users.Commands.Login;
using Assura.Application.Features.Users.Commands.ForgotPassword;
using Assura.Application.Features.Users.Commands.ResetPassword;
using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Assura.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IIdentifyServices _identifyServices;

    public AuthController(IMediator mediator, IIdentifyServices identifyServices)
    {
        _mediator = mediator;
        _identifyServices = identifyServices;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var result = await _mediator.Send(command);
        return result 
            ? Ok(new { Message = "User registration successful. Pending HR assignment." }) 
            : BadRequest(new { Message = "User already exists or registration failed." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
    {
        Console.WriteLine($"[DEBUG] AuthController: Login request received for {command.Username}");
        try {
            var result = await _mediator.Send(command);
            if (result != null) {
                Console.WriteLine("[DEBUG] AuthController: Login successful, returning 200 OK");
                return Ok(result);
            } else {
                Console.WriteLine("[DEBUG] AuthController: Login failed (null result), returning 401 Unauthorized");
                return Unauthorized(new { Message = "Invalid username or password." });
            }
        } catch (Exception ex) {
            Console.WriteLine($"[DEBUG] AuthController: Error during login: {ex.Message}");
            throw;
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        var token = await _mediator.Send(command);
        // In production, we wouldn't return the token.
        // But for development/testing, returning it helps if you don't have email setup.
        return Ok(new { Message = "If an account exists with that email, a reset link has been sent.", Token = token });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        var result = await _mediator.Send(command);
        return result 
            ? Ok(new { Message = "Password has been successfully reset." }) 
            : BadRequest(new { Message = "Invalid token or email, or token expired." });
    }
}
