using Microsoft.AspNetCore.Mvc;
using MediatR;
using Assura.Application.Features.Assets.Queries;
using Microsoft.AspNetCore.Authorization;

namespace Assura.API.Controllers;


/// Asset Pool Controller - Manages asset pool queries and filtering
[ApiController]
[Route("api/asset-pool")]
[Authorize]
public class AssetPoolController : ControllerBase
{
    private readonly IMediator _mediator;

    public AssetPoolController(IMediator mediator)
    {
        _mediator = mediator;
    }

  // Retrieves a paginated list of assets in the pool with optional filters 
  
    [HttpGet]
    public async Task<IActionResult> GetAssetPool(
        [FromQuery] string? search = null,
        [FromQuery] string? category = null,
        [FromQuery] string? division = null,
        [FromQuery] int? employeeId = null,
        [FromQuery] string? specName = null,
        [FromQuery] string? specValue = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var query = new GetAssetPoolQuery(
                Search: search,
                Category: category,
                Division: division,
                EmployeeId: employeeId,
                SpecName: specName,
                SpecValue: specValue,
                Page: page,
                PageSize: pageSize
            );

            var result = await _mediator.Send(query);

            return Ok(new
            {
                success = true,
                data = result
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                success = false,
                error = "Invalid filter parameters",
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error retrieving asset pool: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            return StatusCode(500, new
            {
                success = false,
                error = "Internal server error",
                message = ex.Message,
                details = ex.InnerException?.Message
            });
        }
    }

   
    // Retrieves list of unique employees who have assets assigned to them.
   
    [HttpGet("employees")]
    public async Task<IActionResult> GetAssignedEmployees()
    {
        try
        {
            var query = new GetAssignedEmployeesQuery();
            var result = await _mediator.Send(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving assigned employees: {ex.Message}");
            return StatusCode(500, new
            {
                success = false,
                error = "Internal server error",
                message = ex.Message
            });
        }
    }

  
    // Retrieves list of unique divisions that have assigned assets.
    
    [HttpGet("divisions")]
    public async Task<IActionResult> GetAssignedDivisions()
    {
        try
        {
            var query = new GetAssignedDivisionsQuery();
            var result = await _mediator.Send(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error retrieving assigned divisions: {ex.Message}");
            return StatusCode(500, new
            {
                success = false,
                error = "Internal server error",
                message = ex.Message
            });
        }
    }
}
