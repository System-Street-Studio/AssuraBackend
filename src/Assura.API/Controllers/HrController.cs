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
    public async Task<ActionResult<List<PendingHrUserDto>>> GetPendingUsers()
    {
        return await Mediator.Send(new GetPendingHrUsersQuery());
    }

    [HttpGet("assigned-users")]
    public async Task<ActionResult<List<AssignedHrUserDto>>> GetAssignedUsers()
    {
        return await Mediator.Send(new GetAssignedHrUsersQuery());
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
