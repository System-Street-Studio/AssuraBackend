using System.Security.Claims;
using Assura.Application.DTOs;
using Assura.Application.Features.SystemAdmin.Commands;
using Assura.Application.Features.SystemAdmin.Queries;
using Assura.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assura.API.Controllers;

[Authorize(Roles = $"{Roles.Admin},{Roles.SystemAdmin}")]
public class SystemAdminController : BaseApiController
{
    [HttpGet("dashboard")]
    public async Task<ActionResult<SystemAdminDashboardDto>> GetDashboardStats()
    {
        var result = await Mediator.Send(new GetSystemAdminDashboardQuery());
        return Ok(result);
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<SystemAdminUserDto>>> GetUsers()
    {
        var result = await Mediator.Send(new GetSystemUsersQuery());
        return Ok(result);
    }

    [HttpGet("security-logs")]
    public async Task<ActionResult<List<SystemAdminAuditLogDto>>> GetSecurityLogs()
    {
        var result = await Mediator.Send(new GetSecurityLogsQuery());
        return Ok(result);
    }

    [HttpPut("users/{id}/toggle-lock")]
    public async Task<IActionResult> ToggleUserLock(int id)
    {
        var callerUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(callerUserIdStr, out var callerUserId))
        {
            return Unauthorized();
        }

        var success = await Mediator.Send(new ToggleUserLockCommand(id, callerUserId));
        if (!success) return BadRequest("Failed to toggle user lock status.");
        return Ok();
    }

    [HttpGet("backup-sql")]
    public async Task<IActionResult> GetDatabaseBackupSql([FromServices] Assura.Application.Common.Interfaces.IDatabaseBackupService backupService)
    {
        var backupBytes = await backupService.GenerateSqlBackupAsync();
        var fileName = $"Assura_Backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}.sql";
        return File(backupBytes, "application/sql", fileName);
    }

    [HttpGet("error-logs")]
    public async Task<IActionResult> GetSystemErrorLogs()
    {
        return Ok(await Mediator.Send(new GetSystemLogsQuery()));
    }

    [HttpPost("users/{id}/reset-password")]
    public async Task<IActionResult> ResetUserPassword(int id)
    {
        var callerUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(callerUserIdStr, out var callerUserId))
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new ResetUserPasswordCommand(id, callerUserId));
        if (!result.Success) return BadRequest("Failed to reset user password. Cannot reset system admin.");
        return Ok(new { temporaryPassword = result.TemporaryPassword });
    }

    // Creating privileged accounts (System Admin registration, HR credential generation) is a
    // SystemAdmin-only process — Admin is deliberately excluded here, narrowing the controller's
    // broader Admin-or-SystemAdmin policy just for these two actions.
    [Authorize(Roles = Roles.SystemAdmin)]
    [HttpPost("users/create-hr")]
    public async Task<IActionResult> CreateHrAccount()
    {
        var result = await Mediator.Send(new CreateHrAccountCommand
        {
            ActorName = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.NameIdentifier),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        });

        if (!result.Success) return BadRequest(result.Error);
        return Ok(new { username = result.Username, temporaryPassword = result.TemporaryPassword });
    }

    [Authorize(Roles = Roles.SystemAdmin)]
    [HttpPost("users/create-system-admin")]
    public async Task<IActionResult> CreateSystemAdminUser(CreatePrivilegedUserRequest request)
    {
        var result = await Mediator.Send(new CreatePrivilegedUserCommand
        {
            Username = request.Username,
            Password = request.Password,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            ActorName = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.NameIdentifier),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        });

        if (!result.Success) return BadRequest(result.Error);
        return Ok(new { userId = result.UserId });
    }
}

public record CreatePrivilegedUserRequest(
    string Username,
    string Password,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber);
