using MediatR;
using Microsoft.AspNetCore.Mvc;
using Assura.Application.Features.AccDiscardedItems.Queries.GetAll;
using Assura.Application.Features.AccDiscardedItems.DTOs;

namespace Assura.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccDiscardedItemsController : ControllerBase
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
