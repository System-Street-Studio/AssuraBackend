using Microsoft.AspNetCore.Mvc;
using MediatR;
using Assura.Application.Features.AssetRequests.Commands;
using Assura.Domain.Entities;
using Assura.Application.Features.AssetRequests.Queries;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Assura.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AssetRequestsController : ControllerBase
{
   private readonly IMediator _mediator;

    public AssetRequestsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Creates a new asset request.
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAssetRequestCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(id);
    }

    // Approves an asset request by its ID.
    [HttpPut("{id}/approve")]
    public async Task<ActionResult<bool>> Approve(int id)
    {
        var result = await _mediator.Send(new ApproveAssetRequestCommand(id));
        return Ok(result);
    }

    // Rejects an asset request by its ID.
    [HttpPut("{id}/reject")] 
    public async Task<ActionResult<bool>> Reject(int id)
    {
        
        var result = await _mediator.Send(new RejectAssetRequestCommand(id));
        return Ok(result);
    }

    // Retrieves all asset requests made by a specific employee.
    [HttpGet("employee/{employeeId}")] 
    public async Task<IActionResult> GetByEmployee(string employeeId)
    {
        var result = await _mediator.Send(new GetAllRequestsQuery(employeeId));
        return Ok(result);
    }

    // Retrieves all asset requests that are pending approval for a specific division head.
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;


        // Admin/Procurement/Storekeeper see all pending requests
        if (role == "Admin" || role == "Procurement" || role == "Storekeeper")
        {
            return Ok(await _mediator.Send(new GetPendingRequestsQuery()));
        }
        
        // Safety: if we can't identify user, return empty
        if (string.IsNullOrEmpty(userId))
        {
            return Ok(new List<object>());
        }

        // DivisionHead sees division requests, Employee sees only their own
        var isDivisionHead = role == "DivisionHead";
        var result = await _mediator.Send(new GetPendingRequestsQuery(userId, isDivisionHead));
        return Ok(result);
    }

    // Retrieves all asset requests with optional filters for status and type.
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status = null, [FromQuery] string? type = null, [FromQuery] bool isDivisionHead = false)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        // If Admin/Procurement/Storekeeper, they should see all requests
        if (role == "Admin" || role == "Procurement" || role == "Storekeeper")
        {
            var result = await _mediator.Send(new GetFilteredAssetRequestsQuery(status, type));
            return Ok(result);
        }

        // Safety: if we can't identify user, return empty
        if (string.IsNullOrEmpty(userId))
        {
            return Ok(new List<object>());
        }
        
        var isHead = role == "DivisionHead";
        var filteredResult = await _mediator.Send(new GetFilteredAssetRequestsQuery(status, type, userId, isHead));
        return Ok(filteredResult);
    }

        // Retrieves all approved asset transfer requests for a specific division.
    [HttpGet("approved-transfers")]
    public async Task<IActionResult> GetApprovedTransfers([FromQuery] int? headId = null)
    {
        var result = await _mediator.Send(new GetApprovedTransfersQuery(headId));
        return Ok(result);
    }

    // Retrieves a specific asset request by its ID.
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetAssetRequestByIdQuery { Id = id });
        
        if (result == null)
        {
            return NotFound();
        }
        
        return Ok(result);
    }
    
}