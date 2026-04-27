using MediatR;
using Microsoft.AspNetCore.Mvc;
using Assura.Application.Features.AccDiscardNotes.Queries.GetAll;
using Assura.Application.Features.AccDiscardNotes.DTOs;

namespace Assura.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccDiscardNotesController : ControllerBase
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
