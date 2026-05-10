using Assura.Application.DTOs;
using Assura.Application.Features.Divisions.Queries;
using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace Assura.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DivisionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DivisionsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
// Retrieves a list of all divisions
    [HttpGet]
    public async Task<ActionResult<List<DivisionDto>>> GetDivisions()
    {
        return await _mediator.Send(new GetDivisionsQuery());
    }
}
