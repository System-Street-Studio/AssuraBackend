using Assura.Application.DTOs;
using Assura.Application.NewArrivals.Commands;
using Assura.Application.NewArrivals.Queries;
using Assura.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assura.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class InformingController : ControllerBase
{
    private readonly IMediator _mediator;

    public InformingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpGet("history")]
    public async Task<ActionResult<List<AssetInforming>>> GetHistory()
    {
        var result = await _mediator.Send(new GetAssetInformingsQuery());
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("inform-stores")]
    public async Task<ActionResult<int>> InformStores(InformStoresDto dto)
    {
        var result = await _mediator.Send(new InformStoresCommand(dto));
        return Ok(result);
    }
}
