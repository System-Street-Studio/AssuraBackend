using Assura.Application.Features.Users.Commands.RegisterUser;
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
    public async Task<IActionResult> Register(RegisterUserCommand command)
    {
        var result = await _mediator.Send(command);
        return result 
            ? Ok(new { Message = "User registration successful. Pending HR assignment." }) 
            : BadRequest(new { Message = "User already exists or registration failed." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _identifyServices.AuthenticateAsync(request.Username, request.Password);
        return result != null 
            ? Ok(result) 
            : Unauthorized(new { Message = "Invalid username or password." });
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
