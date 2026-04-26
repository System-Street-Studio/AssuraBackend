using Assura.Application.Features.Maintenances.Queries;
using Assura.Application.Features.Maintenances.Commands;
using Assura.Application.DTOs;
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

    [HttpGet("{id}")]
    public async Task<ActionResult<MaintenanceDto>> GetMaintenance(int id)
    {
        _logger.LogInformation("[DEBUG] MaintenancesController: GetMaintenance called for ID {Id}", id);
        var result = await _mediator.Send(new GetMaintenanceByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult> UpdateStatus(int id, [FromBody] UpdateMaintenanceStatusDto request)
    {
        _logger.LogInformation("[DEBUG] MaintenancesController: UpdateStatus called for ID {Id} with Status {Status}", id, request.Status);
        var result = await _mediator.Send(new UpdateMaintenanceStatusCommand(id, request.Status));
        if (!result) return NotFound();
        return NoContent();
    }
}
