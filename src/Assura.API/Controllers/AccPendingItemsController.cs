using MediatR;
using Microsoft.AspNetCore.Mvc;
using Assura.Application.Features.AccPendingItems.Queries.GetAll;
using Assura.Application.Features.AccPendingItems.Commands.ConfirmDiscard;
using Assura.Application.Features.AccPendingItems.DTOs;

namespace Assura.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccPendingItemsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccPendingItemsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<AccPendingItemDto>>> GetAll()
    {
        var result = await _mediator.Send(new GetAllAccPendingItemsQuery());
        return Ok(result);
    }

    [HttpPost("{id}/discard")]
    public async Task<ActionResult> ConfirmDiscard(int id)
    {
        var result = await _mediator.Send(new ConfirmDiscardCommand { Id = id });
        if (!result) return NotFound();
        return NoContent();
    }
}
