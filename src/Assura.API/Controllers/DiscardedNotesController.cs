using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Assura.Application.Features.DiscardedNotes.Queries.GetAll;
using Assura.Application.Features.DiscardedNotes.Commands.UpdateStatus;
using Assura.Application.Features.DiscardedNotes.DTOs;
using Assura.Domain.Constants;

namespace Assura.API.Controllers;

[Authorize(Roles = $"{Roles.Superintendent},{Roles.Admin}")]
public class DiscardedNotesController : BaseApiController
{
    private readonly IMediator _mediator;

    public DiscardedNotesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<DiscardedNoteDto>>> GetAll()
    {
        var result = await _mediator.Send(new GetAllDiscardedNotesQuery());
        return Ok(result);
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult> UpdateStatus(int id, [FromBody] UpdateDiscardedNoteStatusCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        if (!result) return NotFound();
        return NoContent();
    }
}
