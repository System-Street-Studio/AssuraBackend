using Assura.Application.Features.Users.Queries;
using Assura.Application.Features.Users.Commands.UpdateUserProfile;
using Assura.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Assura.API.Controllers;


[Authorize]
[ApiController]
[Route("api/users")]

public class UserController : ControllerBase
{

    private readonly IMediator _mediator;
    public UserController(IMediator mediator)
    {

        _mediator = mediator;

    }

  

    // Retrieves a list of users who can be assigned assets.
    [HttpGet("assignable-users")]
    public async Task<IActionResult> GetAssignableUsers()
    {

        var result = await _mediator.Send(new GetAssignableUsersQuery());

        return Ok(result);

    }


    // Retrieves the profile of the currently authenticated user.
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()

    {

        Console.WriteLine("[DEBUG] UserController: GetProfile request received");

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        Console.WriteLine($"[DEBUG] UserController: NameIdentifier claim: {userIdClaim}");

        

        if (string.IsNullOrEmpty(userIdClaim)) {

             userIdClaim = User.FindFirst("id")?.Value ?? User.FindFirst("sub")?.Value;

             Console.WriteLine($"[DEBUG] UserController: Alternative ID claims (id/sub): {userIdClaim}");

        }

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))

        {

            return Unauthorized();

        }

        Console.WriteLine($"[DEBUG] UserController: Sending GetUserProfileQuery for UserId: {userId}");
        var profile = await _mediator.Send(new GetUserProfileQuery(userId));

        Console.WriteLine($"[DEBUG] UserController: Profile result: {(profile != null ? "Success" : "Not Found")}");

        if (profile == null) return NotFound();

        return Ok(profile);

    }


   // Updates the profile of the currently authenticated user.
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileCommand command)

    {

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {

            return Unauthorized();

        }

        if (userId != command.UserId)
        {

            return BadRequest("ID mismatch");

        }

        var result = await _mediator.Send(command);
        return result.Success ? Ok() : BadRequest(result.ErrorMessage);

    }

}
