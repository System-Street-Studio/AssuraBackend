using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Assura.Application.Features.LostItems.Queries.GetAll;
using Assura.Application.Features.LostItems.DTOs;
using Assura.Domain.Constants;

namespace Assura.API.Controllers;

[Authorize(Roles = $"{Roles.Accountant},{Roles.Admin}")]
public class LostItemsController : BaseApiController
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
