using Assura.Application.Features.Maintenances.Queries;
using Assura.Application.Features.Maintenances.Commands;
using Assura.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assura.API.Controllers;

[Authorize(Roles = $"{Roles.Procurement},{Roles.Admin},{Roles.Maintenance}")]
public class MaintenancesController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<MaintenancesController> _logger;

    public MaintenancesController(IMediator mediator, ILogger<MaintenancesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<MaintenanceDto>>> GetMaintenances()
    {
        _logger.LogInformation("[DEBUG] MaintenancesController: GetMaintenances called");
        return await _mediator.Send(new GetMaintenancesQuery());
    }

    [HttpPost]
    public async Task<ActionResult<int>> CreateMaintenance([FromBody] CreateMaintenanceCommand command)
    {
        _logger.LogInformation("[DEBUG] MaintenancesController: CreateMaintenance called with {@Command}", command);
        return await _mediator.Send(command);
    }
}
