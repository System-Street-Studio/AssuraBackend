using Assura.Application.DTOs;
using Assura.Application.Features.Divisions.Commands;
using Assura.Application.Features.Divisions.Queries;
using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace Assura.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DivisionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DivisionsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
// Retrieves a list of all divisions
    [HttpGet]
    public async Task<ActionResult<List<DivisionDto>>> GetDivisions()
    {
        return await _mediator.Send(new GetDivisionsQuery());
    }

    // GET: api/divisions/{divisionId}/overview-summary
    [HttpGet("{divisionId}/overview-summary")]
    public async Task<ActionResult<DivisionOverviewSummaryDto>> GetDivisionOverviewSummary(int divisionId)
    {
        var summary = await _mediator.Send(new GetDivisionOverviewSummaryQuery(divisionId));
        return Ok(summary);
    }

    [HttpPost]
    public async Task<ActionResult<DivisionDto>> CreateDivision([FromBody] CreateDivisionCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetDivisions), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DivisionDto>> UpdateDivision(int id, [FromBody] UpdateDivisionCommand command)
    {
        if (id != command.Id) return BadRequest();
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteDivision(int id)
    {
        await _mediator.Send(new DeleteDivisionCommand(id));
        return NoContent();
    }
}
