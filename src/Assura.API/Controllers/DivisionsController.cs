using Assura.Application.DTOs;
using Assura.Application.Features.Divisions.Queries;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;

namespace Assura.API.Controllers;

[AllowAnonymous]
public class DivisionsController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<List<DivisionDto>>> GetDivisions()
    {
        return await Mediator.Send(new GetDivisionsQuery());
    }
}
