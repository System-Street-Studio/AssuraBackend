using System.Security.Claims;
using Assura.Application.Features.HR.Commands;
using Assura.Application.Features.HR.Queries;
using Assura.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assura.API.Controllers;

[Authorize(Roles = $"{Roles.HR},{Roles.Admin}")]
public class HrController : BaseApiController
{
    [HttpGet("overview")]
    public async Task<ActionResult<HrOverviewDto>> GetOverview()
    {
        return await Mediator.Send(new GetHrOverviewQuery());
    }

    [HttpGet("pending-users")]
    public async Task<ActionResult<List<PendingHrUserDto>>> GetPendingUsers([FromQuery] string? search = null)
    {
        return await Mediator.Send(new GetPendingHrUsersQuery(search));
    }

    [HttpGet("assigned-users")]
    public async Task<ActionResult<List<AssignedHrUserDto>>> GetAssignedUsers(
        [FromQuery] string? search = null,
        [FromQuery] string? division = null,
        [FromQuery] string? role = null)
    {
        return await Mediator.Send(new GetAssignedHrUsersQuery(search, division, role));
    }

    [HttpGet("users/{userId:int}")]
    public async Task<ActionResult<HrUserDetailDto>> GetUserById(int userId)
    {
        var result = await Mediator.Send(new GetHrUserByIdQuery(userId));
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("activity-logs")]
    public async Task<ActionResult<List<HrActivityLogDto>>> GetActivityLogs([FromQuery] string? search = null)
    {
        return await Mediator.Send(new GetHrActivityLogsQuery(search));
    }

    [HttpPost("users/{userId:int}/assign-role")]
    public async Task<IActionResult> AssignRole(int userId, [FromBody] AssignHrRoleRequest request)
    {
        var command = new AssignHrRoleCommand
        {
            UserId = userId,
            Role = request.Role,
            DivisionId = request.DivisionId,
            JobTitle = request.JobTitle,
            Notes = request.Notes,
            ActorName = ResolveActorName(),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Device = Request.Headers.UserAgent.ToString()
        };

        var result = await Mediator.Send(command);
        return result
            ? Ok(new { message = "Role assigned successfully." })
            : BadRequest(new { message = "Unable to assign role." });
    }

    [HttpPut("users/{userId:int}")]
    public async Task<IActionResult> UpdateUser(int userId, [FromBody] UpdateHrUserRequest request)
    {
        var command = new UpdateHrUserCommand
        {
            UserId = userId,
            DivisionId = request.DivisionId,
            Role = request.Role,
            JobTitle = request.JobTitle,
            PhoneNumber = request.PhoneNumber,
            RequestedRole = request.RequestedRole,
            EmploymentStatus = request.EmploymentStatus,
            Notes = request.Notes,
            ActorName = ResolveActorName(),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Device = Request.Headers.UserAgent.ToString()
        };

        var result = await Mediator.Send(command);
        return result
            ? Ok(new { message = "User updated successfully." })
            : BadRequest(new { message = "Unable to update user." });
    }

    [HttpPost("users/{userId:int}/reject")]
    public async Task<IActionResult> RejectUser(int userId, [FromBody] RejectHrUserRequest request)
    {
        var command = new RejectHrUserCommand
        {
            UserId = userId,
            Notes = request.Notes,
            ActorName = ResolveActorName(),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Device = Request.Headers.UserAgent.ToString()
        };

        var result = await Mediator.Send(command);
        return result
            ? Ok(new { message = "User rejected successfully." })
            : BadRequest(new { message = "Unable to reject user." });
    }

    private string ResolveActorName()
    {
        return User.Identity?.Name
            ?? User.FindFirst(ClaimTypes.Name)?.Value
            ?? User.FindFirst(ClaimTypes.Email)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? "HR Officer";
    }
}

public class AssignHrRoleRequest
{
    public string Role { get; set; } = string.Empty;
    public int? DivisionId { get; set; }
    public string? JobTitle { get; set; }
    public string? Notes { get; set; }
}

public class UpdateHrUserRequest
{
    public int? DivisionId { get; set; }
    public string? Role { get; set; }
    public string? JobTitle { get; set; }
    public string? PhoneNumber { get; set; }
    public string? RequestedRole { get; set; }
    public string? EmploymentStatus { get; set; }
    public string? Notes { get; set; }
}

public class RejectHrUserRequest
{
    public string? Notes { get; set; }
}
