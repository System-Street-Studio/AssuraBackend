using MediatR;
using Microsoft.AspNetCore.Mvc;
using Assura.Application.Features.Receipts.Queries.GetAll;
using Assura.Application.Features.Receipts.Commands.Create;
using Assura.Application.Features.Receipts.DTOs;

namespace Assura.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReceiptsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReceiptsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<ReceiptDto>>> GetAll()
    {
        var result = await _mediator.Send(new GetAllReceiptsQuery());
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ReceiptDto>> Create([FromBody] CreateReceiptCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }
}
