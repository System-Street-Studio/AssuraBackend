using Assura.Application.DTOs;
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
}
