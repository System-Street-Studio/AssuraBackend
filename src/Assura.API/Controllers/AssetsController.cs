using Assura.Application.Features.Assets.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Assura.API.Controllers;

public class AssetsController : BaseApiController
{
    private readonly IMediator _mediator;

    public AssetsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<AssetSummaryDto>>> GetAssets()
    {
        return await _mediator.Send(new GetAssetsQuery());
    }
}
