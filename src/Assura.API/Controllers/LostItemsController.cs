using MediatR;
using Microsoft.AspNetCore.Mvc;
using Assura.Application.Features.LostItems.Queries.GetAll;
using Assura.Application.Features.LostItems.DTOs;

namespace Assura.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LostItemsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LostItemsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<LostItemDto>>> GetAll()
    {
        var result = await _mediator.Send(new GetAllLostItemsQuery());
        return Ok(result);
    }
}
