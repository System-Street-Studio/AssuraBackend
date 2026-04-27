using MediatR;
using Microsoft.AspNetCore.Mvc;
using Assura.Application.Features.Buyers.Queries.GetAll;
using Assura.Application.Features.Buyers.DTOs;

namespace Assura.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BuyersController : ControllerBase
{
    private readonly IMediator _mediator;

    public BuyersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<BuyerDto>>> GetAll()
    {
        var result = await _mediator.Send(new GetAllBuyersQuery());
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<string>> Create([FromBody] Assura.Application.Features.Buyers.Commands.CreateBuyer.CreateBuyerCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(id);
    }
}
