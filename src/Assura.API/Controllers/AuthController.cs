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
        try {
            var result = await _mediator.Send(command);
            return result != null
                ? Ok(result)
                : Unauthorized(new { Message = "Invalid username or password." });
        } catch (UnauthorizedAccessException ex) {
            return Unauthorized(new { Message = ex.Message });
        }
    }

    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            await _identifyServices.LogoutAsync(userId);
        }
        return Ok(new { Message = "Logged out successfully." });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        await _mediator.Send(command);
        // Always return a generic message and never the token itself - the reset token
        // must only ever reach the user via the email channel, or anyone who knows a
        // victim's email could reset their password without touching their inbox.
        return Ok(new { Message = "If an account exists with that email, a reset link has been sent." });
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
