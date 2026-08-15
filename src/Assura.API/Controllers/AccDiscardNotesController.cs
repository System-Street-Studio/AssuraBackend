using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Assura.Application.Features.AccDiscardNotes.Queries.GetAll;
using Assura.Application.Features.AccDiscardNotes.DTOs;
using Assura.Domain.Constants;

namespace Assura.API.Controllers;

[Authorize(Roles = $"{Roles.Accountant},{Roles.Admin}")]
public class AccDiscardNotesController : BaseApiController
{
    private readonly IMediator _mediator;

    public AccDiscardNotesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<AccDiscardNoteDto>>> GetAll()
    {
        var result = await _mediator.Send(new GetAllAccDiscardNotesQuery());
        return Ok(result);
    }
}
