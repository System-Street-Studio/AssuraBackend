using Assura.Application.DTOs;
using Assura.Application.NewArrivals.Commands;
using Assura.Application.NewArrivals.Queries;
using Assura.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assura.API.Controllers;

[Authorize]
public class InformingController : BaseApiController
{
    private readonly IMediator _mediator;

    public InformingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("history")]
    public async Task<ActionResult<List<AssetInformingDto>>> GetHistory()
    {
        var result = await _mediator.Send(new GetAssetInformingsQuery());
        Console.WriteLine($"[DEBUG] InformingController: GetHistory returned {result.Count} items.");
        return Ok(result);
    }

    [HttpPost("inform-stores")]
    public async Task<ActionResult<int>> InformStores(InformStoresDto dto)
    {
        var result = await _mediator.Send(new InformStoresCommand(dto));
        return Ok(result);
    }

    [HttpPost("inform-stakeholders")]
    public async Task<ActionResult<int>> InformStakeholders(InformStakeholdersDto dto)
    {
        var result = await _mediator.Send(new InformStakeholdersCommand(dto));
        return Ok(result);
    }

    [HttpGet("my-arrivals")]
    public async Task<ActionResult<List<AssetInformingDto>>> GetMyArrivals([FromQuery] int? divisionId)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out int userId))
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new GetEmployeeArrivalsQuery(userId, divisionId));
        return Ok(result);
    }

    [HttpPost("{id}/confirm")]
    public async Task<ActionResult<bool>> ConfirmArrival(int id, [FromBody] ConfirmArrivalRequest? request)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out int userId))
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new ConfirmAssetArrivalCommand(id, userId, request?.Remarks));
        return Ok(result);
    }

    [HttpPost("{id}/complete")]
    public async Task<ActionResult<bool>> CompleteArrival(int id, [FromBody] ConfirmArrivalRequest? request)
    {
        var result = await _mediator.Send(new CompleteAssetArrivalCommand(id, request?.Remarks));
        return Ok(result);
    }
}

public class ConfirmArrivalRequest
{
    public string? Remarks { get; set; }
}
