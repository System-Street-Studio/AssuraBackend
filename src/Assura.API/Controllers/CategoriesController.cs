using Assura.Application.DTOs;
using Assura.Application.Features.Categories.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Assura.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly MediatR.IMediator _mediator;

    public CategoriesController(MediatR.IMediator mediator)
    {
        _mediator = mediator;
    }

// Retrieves a list of all asset categories
    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetCategories()
    {
        return await _mediator.Send(new GetCategoriesQuery());
    }
}
