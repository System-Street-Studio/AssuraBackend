using Assura.Application.DTOs;
using Assura.Application.Features.Dashboard.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Assura.API.Controllers;

public class DashboardController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<DashboardDto>> GetDashboard()
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int? userId = int.TryParse(userIdStr, out var id) ? id : null;

        // If user is Employee, we should filter. 
        // For Admin/Storekeeper, we might want to see everything.
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (role != "Admin" && role != "Storekeeper" && role != "Procurement")
        {
            return await Mediator.Send(new GetDashboardQuery(userId));
        }

        return await Mediator.Send(new GetDashboardQuery());
    }
}
