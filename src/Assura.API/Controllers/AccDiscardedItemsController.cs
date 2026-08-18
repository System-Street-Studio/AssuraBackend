using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Assura.Application.Features.AccDiscardedItems.Queries.GetAll;
using Assura.Application.Features.AccDiscardedItems.DTOs;
using Assura.Domain.Constants;

namespace Assura.API.Controllers;

[Authorize(Roles = $"{Roles.Accountant},{Roles.Admin}")]
public class AccDiscardedItemsController : BaseApiController
{
    private readonly IMediator _mediator;

    public AccDiscardedItemsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<AccDiscardedItemDto>>> GetAll()
    {
        var result = await _mediator.Send(new GetAllAccDiscardedItemsQuery());
        return Ok(result);
    }
}
