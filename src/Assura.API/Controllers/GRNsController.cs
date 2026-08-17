using Assura.Application.Features.GRNs.Commands;
using Assura.Application.Features.GRNs.Queries;
using Assura.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assura.API.Controllers;

[Authorize]
public class GRNsController : BaseApiController
{
    private readonly IMediator _mediator;

    public GRNsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<GRNDto>>> GetGRNs()
    {
        return await _mediator.Send(new GetGRNsQuery());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GRNDto>> GetGRN(int id)
    {
        var result = await _mediator.Send(new GetGRNByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Storekeeper}")]
    public async Task<ActionResult<GRNDto>> CreateGRN([FromBody] CreateGRNRequest request)
    {
        var result = await _mediator.Send(new CreateGRNCommand(
            request.PurchasingOrderId,
            request.AssetId,
            request.ReceivedDate,
            request.ReceivedBy,
            request.Notes,
            request.InformingId,
            request.ItemName,
            request.Model));

        return CreatedAtAction(nameof(GetGRN), new { id = result.Id }, result);
    }
}

public class CreateGRNRequest
{
    public int PurchasingOrderId { get; set; }
    public int? AssetId { get; set; }
    public DateTime ReceivedDate { get; set; }
    public string? ReceivedBy { get; set; }
    public string? Notes { get; set; }
    public int? InformingId { get; set; }
    public string? ItemName { get; set; }
    public string? Model { get; set; }
}
