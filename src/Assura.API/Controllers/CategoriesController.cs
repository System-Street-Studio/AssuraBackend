using Assura.Application.DTOs;
using Assura.Application.Features.Categories.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Assura.API.Controllers;

public class CategoriesController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetCategories()
    {
        return await Mediator.Send(new GetCategoriesQuery());
    }
}
