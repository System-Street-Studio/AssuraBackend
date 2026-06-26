using Assura.Application.DTOs;
using Assura.Application.Features.Categories.Commands;
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

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetCategories), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, [FromBody] UpdateCategoryCommand command)
    {
        if (id != command.Id) return BadRequest();
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCategory(int id)
    {
        await _mediator.Send(new DeleteCategoryCommand(id));
        return NoContent();
    }
}
