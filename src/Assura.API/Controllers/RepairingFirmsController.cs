using Assura.Application.Features.RepairingFirms.Queries;
using Assura.Application.Features.RepairingFirms.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Assura.API.Controllers;

public class RepairingFirmsController : BaseApiController
{
    private readonly IMediator _mediator;

    public RepairingFirmsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<RepairingFirmDto>>> GetRepairingFirms()
    {
        return await _mediator.Send(new GetRepairingFirmsQuery());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RepairingFirmDto?>> GetRepairingFirmById(int id)
    {
        var result = await _mediator.Send(new GetRepairingFirmByIdQuery(id));
        if (result == null)
        {
            return NotFound($"Repairing firm with ID {id} not found.");
        }
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<int>> CreateRepairingFirm([FromBody] CreateRepairingFirmCommand command)
    {
        return await _mediator.Send(command);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<bool>> UpdateRepairingFirm(int id, [FromBody] UpdateRepairingFirmCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("ID in URL path does not match ID in request body.");
        }

        var result = await _mediator.Send(command);
        if (!result)
        {
            return NotFound($"Repairing firm with ID {id} not found.");
        }

        return Ok(true);
    }
}
