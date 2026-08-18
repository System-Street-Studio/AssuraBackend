using Assura.Application.Features.Maintenances.Queries;
using Assura.Application.Features.Maintenances.Commands;
using Assura.Application.DTOs;
using Assura.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Assura.API.Controllers;

[Authorize(Roles = $"{Roles.Procurement},{Roles.Admin},{Roles.Maintenance},{Roles.Storekeeper},{Roles.DivisionHead},{Roles.Employee}")]
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
        var (userId, isDivisionHead) = GetCallerIdentity();
        try
        {
            await _mediator.Send(new UpdateMaintenanceStatusCommand(id, "Approved", userId, isDivisionHead));
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPatch("{id:int}/start")]
    public async Task<ActionResult> Start(int id)
    {
        var (userId, isDivisionHead) = GetCallerIdentity();
        try
        {
            await _mediator.Send(new UpdateMaintenanceStatusCommand(id, "InProgress", userId, isDivisionHead));
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPatch("{id:int}/assign-temp")]
    public async Task<ActionResult> AssignTemp(int id, [FromBody] AssignTemporaryAssetCommand command)
    {
        var (userId, isDivisionHead) = GetCallerIdentity();
        command = command with { MaintenanceId = id, StorekeeperUserId = userId, IsDivisionHead = isDivisionHead };
        try
        {
            await _mediator.Send(command);
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPatch("{id:int}/send-for-repair")]
    public async Task<ActionResult> SendForRepair(int id, [FromBody] SendForRepairCommand command)
    {
        var (userId, isDivisionHead) = GetCallerIdentity();
        command = command with { MaintenanceId = id, StorekeeperUserId = userId, IsDivisionHead = isDivisionHead };
        try
        {
            await _mediator.Send(command);
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPatch("{id:int}/escalate-procurement")]
    public async Task<ActionResult> EscalateToProcurement(int id, [FromBody] EscalateToProcurementCommand command)
    {
        var (userId, isDivisionHead) = GetCallerIdentity();
        command = command with { MaintenanceId = id, StorekeeperUserId = userId, IsDivisionHead = isDivisionHead };
        try
        {
            await _mediator.Send(command);
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPatch("{id:int}/inform-stakeholders")]
    [Authorize(Roles = $"{Roles.Storekeeper},{Roles.Admin}")]
    public async Task<ActionResult> InformStakeholders(int id)
    {
        var userId = GetCurrentUserId();
        var result = await _mediator.Send(new InformMaintenanceStakeholdersCommand
        {
            MaintenanceId = id,
            StorekeeperUserId = userId
        });

        return result switch
        {
            InformMaintenanceStakeholdersResult.NotFound => NotFound(),
            InformMaintenanceStakeholdersResult.InvalidStatus => Conflict("Maintenance must be Completed before stakeholders can be informed."),
            _ => Ok()
        };
    }

    [HttpPatch("{id:int}/complete")]
    public async Task<ActionResult> Complete(int id)
    {
        var (userId, isDivisionHead) = GetCallerIdentity();
        try
        {
            await _mediator.Send(new UpdateMaintenanceStatusCommand(id, "Completed", userId, isDivisionHead));
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPatch("{id:int}/reject")]
    public async Task<ActionResult> Reject(int id, [FromBody] RejectMaintenanceCommand command)
    {
        var (userId, isDivisionHead) = GetCallerIdentity();
        command = command with { MaintenanceId = id, RejectedByUserId = userId, IsDivisionHead = isDivisionHead };
        try
        {
            await _mediator.Send(command);
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPut("{id:int}/status")]
    public async Task<ActionResult> UpdateStatus(int id, [FromBody] UpdateMaintenanceStatusRequest request)
    {
        var (userId, isDivisionHead) = GetCallerIdentity();
        try
        {
            await _mediator.Send(new UpdateMaintenanceStatusCommand(id, request.Status, userId, isDivisionHead));
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    private int GetCurrentUserId()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? User.FindFirst("sub")?.Value;
        return int.TryParse(userIdStr, out var id) ? id : 0;
    }

    // Mirrors AssetRequestsController's GetCallerIdentity/division-scoping pattern:
    // DivisionHead callers are scoped to their own division's maintenance records in
    // the command handlers; Admin/Procurement/Storekeeper/Maintenance remain fully
    // privileged (IsDivisionHead is false for them, so handlers skip the check).
    private (int UserId, bool IsDivisionHead) GetCallerIdentity()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? User.FindFirst("sub")?.Value;
        var userId = int.TryParse(userIdStr, out var id) ? id : 0;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        return (userId, role == Roles.DivisionHead);
    }
}

public class UpdateMaintenanceStatusRequest
{
    public string Status { get; set; } = string.Empty;
}
