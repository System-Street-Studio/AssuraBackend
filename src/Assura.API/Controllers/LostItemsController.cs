using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Assura.Application.Features.LostItems.Queries.GetAll;
using Assura.Application.Features.LostItems.Commands.Create;
using Assura.Application.Features.LostItems.Commands.UpdateStatus;
using Assura.Application.Features.LostItems.DTOs;
using Assura.Domain.Constants;

namespace Assura.API.Controllers;

public class LostItemsController : BaseApiController
{
    private readonly IMediator _mediator;

    public LostItemsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Roles = $"{Roles.Superintendent},{Roles.Accountant},{Roles.Admin}")]
    [HttpGet]
    public async Task<ActionResult<List<LostItemDto>>> GetAll()
    {
        var result = await _mediator.Send(new GetAllLostItemsQuery());
        return Ok(result);
    }

    [Authorize(Roles = $"{Roles.Employee},{Roles.Storekeeper},{Roles.Superintendent},{Roles.Admin}")]
    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] CreateLostItemCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAll), new { id }, id);
    }

    [Authorize(Roles = $"{Roles.Superintendent},{Roles.Admin}")]
    [HttpPut("{id}/status")]
    public async Task<ActionResult> UpdateStatus(int id, [FromBody] UpdateLostItemStatusCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        if (!result) return NotFound();
        return NoContent();
    }
}
