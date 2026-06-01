using MediatR;
using Microsoft.AspNetCore.Mvc;
using Assura.Application.Features.Assets.Queries.GetAll;
using Assura.Application.Features.Assets.DTOs;

namespace Assura.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssetsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AssetsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<AssetDto>>> GetAll()
    {
        var result = await _mediator.Send(new GetAllAssetsQuery());
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<string>> Create([FromBody] Assura.Application.Features.Assets.Commands.Create.CreateAssetCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] Assura.Application.Features.Assets.Commands.Update.UpdateAssetCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        var result = await _mediator.Send(command);
        if (!result) return NotFound();
        return NoContent();
    }
}
