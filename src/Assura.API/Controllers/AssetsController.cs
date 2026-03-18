using Assura.Application.Features.Assets.Queries;
using Assura.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assura.API.Controllers;

[Authorize(Roles = $"{Roles.Admin},{Roles.Procurement},{Roles.Storekeeper},{Roles.Auditor}")]
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
