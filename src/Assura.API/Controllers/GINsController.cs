using Assura.Application.Features.GINs.Commands;
using Assura.Application.Features.GINs.Queries;
using Assura.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assura.API.Controllers;

[Authorize]
public class GINsController : BaseApiController
{
    private readonly IMediator _mediator;

    public GINsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<GINDto>>> GetGINs()
    {
        return await _mediator.Send(new GetGINsQuery());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GINDto>> GetGIN(int id)
    {
        var result = await _mediator.Send(new GetGINByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Storekeeper}")]
    public async Task<ActionResult<GINDto>> CreateGIN([FromBody] CreateGINRequest request)
    {
        var result = await _mediator.Send(new CreateGINCommand(
            request.GRNId,
            request.AssetId,
            request.AssignedDate,
            request.Condition,
            request.Notes));

        return CreatedAtAction(nameof(GetGIN), new { id = result.Id }, result);
    }
}

public class CreateGINRequest
{
    public int GRNId { get; set; }
    public int AssetId { get; set; }
    public DateTime AssignedDate { get; set; }
    public string? Condition { get; set; }
    public string? Notes { get; set; }
}
