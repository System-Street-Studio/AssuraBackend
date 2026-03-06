using Assura.Application.DTOs;
using Assura.Application.Features.Divisions.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Assura.API.Controllers;

public class DivisionsController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<List<DivisionDto>>> GetDivisions()
    {
        return await Mediator.Send(new GetDivisionsQuery());
    }
}
