using Assura.Application.Features.Maintenances.Queries;
using Assura.Application.Features.Maintenances.Commands;
using Assura.Application.DTOs;
using Assura.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Assura.API.Controllers;

[Authorize(Roles = $"{Roles.Procurement},{Roles.Admin},{Roles.Maintenance},{Roles.Storekeeper},{Roles.DivisionHead}")]
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

    [HttpGet("stats")]
    public async Task<ActionResult<MaintenanceStatsDto>> GetStats()
    {
        return await _mediator.Send(new GetMaintenanceStatsQuery());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MaintenanceDto>> GetById(int id)
    {
        var result = await _mediator.Send(new GetMaintenanceByIdQuery(id));
        if (result == null) return NotFound();
        return result;
    }

    [HttpGet("{id:int}/similar-assets")]
    public async Task<ActionResult<List<SimilarAssetDto>>> GetSimilarAssets(int id)
    {
        return await _mediator.Send(new GetSimilarAssetsQuery(id));
    }

    [HttpPost]
    public async Task<ActionResult<int>> CreateMaintenance([FromBody] CreateMaintenanceCommand command)
    {
        _logger.LogInformation("[DEBUG] MaintenancesController: CreateMaintenance called with {@Command}", command);
        return await _mediator.Send(command);
    }

    [HttpPatch("{id:int}/approve")]
    public async Task<ActionResult> Approve(int id)
    {
        var userId = GetCurrentUserId();
        await _mediator.Send(new UpdateMaintenanceStatusCommand(id, "Approved", userId));
        return Ok();
    }

    [HttpPatch("{id:int}/start")]
    public async Task<ActionResult> Start(int id)
    {
        var userId = GetCurrentUserId();
        await _mediator.Send(new UpdateMaintenanceStatusCommand(id, "InProgress", userId));
        return Ok();
    }

    [HttpPatch("{id:int}/assign-temp")]
    public async Task<ActionResult> AssignTemp(int id, [FromBody] AssignTemporaryAssetCommand command)
    {
        command = command with { MaintenanceId = id, StorekeeperUserId = GetCurrentUserId() };
        await _mediator.Send(command);
        return Ok();
    }

    [HttpPatch("{id:int}/send-for-repair")]
    public async Task<ActionResult> SendForRepair(int id, [FromBody] SendForRepairCommand command)
    {
        command = command with { MaintenanceId = id, StorekeeperUserId = GetCurrentUserId() };
        await _mediator.Send(command);
        return Ok();
    }

    [HttpPatch("{id:int}/escalate-procurement")]
    public async Task<ActionResult> EscalateToProcurement(int id, [FromBody] EscalateToProcurementCommand command)
    {
        command = command with { MaintenanceId = id, StorekeeperUserId = GetCurrentUserId() };
        await _mediator.Send(command);
        return Ok();
    }

    [HttpPatch("{id:int}/complete")]
    public async Task<ActionResult> Complete(int id)
    {
        var userId = GetCurrentUserId();
        await _mediator.Send(new UpdateMaintenanceStatusCommand(id, "Completed", userId));
        return Ok();
    }

    [HttpPatch("{id:int}/reject")]
    public async Task<ActionResult> Reject(int id, [FromBody] RejectMaintenanceCommand command)
    {
        command = command with { MaintenanceId = id, RejectedByUserId = GetCurrentUserId() };
        await _mediator.Send(command);
        return Ok();
    }

    private int GetCurrentUserId()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? User.FindFirst("sub")?.Value;
        return int.TryParse(userIdStr, out var id) ? id : 0;
    }
}
