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
        return await Mediator.Send(new GetDashboardQuery());
    }
}
