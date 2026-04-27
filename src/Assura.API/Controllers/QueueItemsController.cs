using MediatR;
using Microsoft.AspNetCore.Mvc;
using Assura.Application.Features.QueueItems.Queries.GetAll;
using Assura.Application.Features.QueueItems.Commands.UpdateStatus;
using Assura.Application.Features.QueueItems.DTOs;

namespace Assura.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QueueItemsController : ControllerBase
{
    private readonly IMediator _mediator;

    public QueueItemsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<QueueItemDto>>> GetAll()
    {
        var result = await _mediator.Send(new GetAllQueueItemsQuery());
        return Ok(result);
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult> UpdateStatus(int id, [FromBody] UpdateQueueItemStatusCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        if (!result) return NotFound();
        return NoContent();
    }
}
