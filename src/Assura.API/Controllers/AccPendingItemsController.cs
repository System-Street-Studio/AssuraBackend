using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Assura.Application.Features.AccPendingItems.Queries.GetAll;
using Assura.Application.Features.AccPendingItems.Commands.ConfirmDiscard;
using Assura.Application.Features.AccPendingItems.DTOs;
using Assura.Domain.Constants;

namespace Assura.API.Controllers;

[Authorize(Roles = $"{Roles.Accountant},{Roles.Admin}")]
public class AccPendingItemsController : BaseApiController
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
    public async Task<ActionResult> ConfirmDiscard(int id, [FromBody] ConfirmDiscardApiInput input)
    {
        var result = await _mediator.Send(new ConfirmDiscardCommand { Id = id, ReceiptId = input.ReceiptId });
        if (!result) return NotFound();
        return NoContent();
    }

    public class ConfirmDiscardApiInput
    {
        public int ReceiptId { get; set; }
    }
}
