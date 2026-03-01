using Assura.Application.DTOs;
using Assura.Application.Features.Divisions.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Assura.API.Controllers;

/// <summary>
/// Manages organizational divisions.
/// </summary>
public class DivisionsController : BaseApiController
{
    /// <summary>
    /// Retrieves all divisions.
    /// </summary>
    /// <returns>A list of divisions.</returns>
    /// <response code="200">Returns the list of divisions.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<DivisionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DivisionDto>>> GetDivisions()
    {
        return await Mediator.Send(new GetDivisionsQuery());
    }
}
