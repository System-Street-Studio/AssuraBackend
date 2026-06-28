using Assura.Application.DTOs;
using Assura.Application.Features.SystemAdmin.Commands;
using Assura.Application.Features.SystemAdmin.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Assura.API.Controllers;

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
        var success = await Mediator.Send(new ToggleUserLockCommand(id));
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
        var success = await Mediator.Send(new ResetUserPasswordCommand(id));
        if (!success) return BadRequest("Failed to reset user password. Cannot reset system admin.");
        return Ok();
    }
}
