using Microsoft.AspNetCore.Mvc;
using Assura.Application.DTOs;
using Assura.Application.Features.AssetSpecifications.Queries;

namespace Assura.API.Controllers;

public class AssetSpecificationsController : BaseApiController
{
    private readonly MediatR.IMediator _mediator;

    public AssetSpecificationsController(MediatR.IMediator mediator)
    {
        _mediator = mediator;
    }

    /// Get all asset specifications
    [HttpGet]
    public async Task<ActionResult<List<AssetSpecificationDto>>> GetAllSpecifications()
    {
        return await _mediator.Send(new GetAssetSpecificationsQuery());
    }

   
    /// Get asset specifications by category ID
    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<List<AssetSpecificationDto>>> GetByCategory(int categoryId)
    {
        var result = await _mediator.Send(new GetAssetSpecificationsQuery(categoryId));
        return Ok(result);
    }

  
    /// Get asset specifications by category name
    [HttpGet("categoryname/{categoryName}")]
    public async Task<ActionResult<List<AssetSpecificationDto>>> GetByCategoryName(string categoryName)
    {
        // Get all specifications
        var allSpecs = await _mediator.Send(new GetAssetSpecificationsQuery());
        
        // Filter by category name
        var filtered = allSpecs
            .Where(s => s.CategoryName.Equals(categoryName, StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        return Ok(filtered);
    }
}
