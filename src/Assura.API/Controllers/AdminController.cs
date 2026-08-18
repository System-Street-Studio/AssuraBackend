using Assura.Application.Admin.Queries;
using Assura.Application.DTOs;
using Assura.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assura.API.Controllers;

// Storekeeper was previously included here only because the mobile app's shared
// "Admin dashboard" screen also let Storekeeper accounts log in. The mobile app is now
// Admin-only (see BUGS.md), so Storekeeper no longer needs access to this endpoint.
[Authorize(Roles = Roles.Admin + "," + Roles.Employee)]
public class AdminController : BaseApiController
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("dashboard-stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
    {
        if (User.IsInRole(Roles.Employee) && !User.IsInRole(Roles.Admin))
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                              ?? User.FindFirst("id")?.Value 
                              ?? User.FindFirst("sub")?.Value;

            if (int.TryParse(userIdClaim, out int userId))
            {
                var profile = await _mediator.Send(new Assura.Application.Features.Users.Queries.GetUserProfileQuery(userId));
                if (profile == null || profile.DivisionName?.ToLower() != "admin")
                {
                    return Forbid();
                }
            }
            else
            {
                return Forbid();
            }
        }

        var result = await _mediator.Send(new GetDashboardStatsQuery());
        return Ok(result);
    }
}
