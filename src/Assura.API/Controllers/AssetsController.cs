using Assura.Application.DTOs;
using Assura.Application.Features.Assets.Commands;
using Assura.Application.Features.Assets.Queries;
using Assura.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assura.API.Controllers;

[AllowAnonymous]
public class AssetsController : BaseApiController
{
    private readonly IMediator _mediator;

    public AssetsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<AssetDto>>> GetAssets()
    {
        return await _mediator.Send(new GetAssetsQuery());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AssetDto>> GetAsset(int id)
    {
        var result = await _mediator.Send(new GetAssetByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<AssetDto>> CreateAsset(AssetCreateDto asset)
    {
        var result = await _mediator.Send(new CreateAssetCommand(asset));
        return CreatedAtAction(nameof(GetAsset), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AssetDto>> UpdateAsset(int id, AssetUpdateDto asset)
    {
        if (id != asset.Id) return BadRequest("ID mismatch");
        var result = await _mediator.Send(new UpdateAssetCommand(asset));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsset(int id)
    {
        var result = await _mediator.Send(new DeleteAssetCommand(id));
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{id}/checkin")]
    public async Task<ActionResult<AssetDto>> CheckinAsset(int id, [FromBody] CheckinRequest request)
    {
        var result = await _mediator.Send(new CheckinAssetCommand(id, request.Condition, request.Notes));
        if (result == null) return NotFound();
        return Ok(result);
    }

    public class CheckinRequest
    {
        public string Condition { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
