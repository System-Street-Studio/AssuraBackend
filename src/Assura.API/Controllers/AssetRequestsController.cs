using Microsoft.AspNetCore.Mvc;
using MediatR;
using Assura.Application.Features.AssetRequests.Commands;
using Assura.Domain.Entities;
using Assura.Application.Features.AssetRequests.Queries;
using System.Security.Claims;

namespace Assura.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssetRequestsController : ControllerBase
{
   private readonly IMediator _mediator;

    public AssetRequestsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAssetRequestCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(id);
    }

    [HttpPut("{id}/approve")]
    public async Task<ActionResult<bool>> Approve(int id)
    {
        var result = await _mediator.Send(new ApproveAssetRequestCommand(id));
        return Ok(result);
    }

    [HttpPut("{id}/reject")] 
    public async Task<ActionResult<bool>> Reject(int id)
    {
        
        var result = await _mediator.Send(new RejectAssetRequestCommand(id));
        return Ok(result);
    }

    /*[HttpPut("{id}/reject")] 
    public async Task<ActionResult<bool>> Reject(int id, [FromBody] RejectRequestDto model)
    {
        
        var result = await _mediator.Send(new RejectAssetRequestCommand(id, model.Reason));
        return Ok(result);
    }*/

    [HttpGet("employee/{employeeId}")] 
    public async Task<IActionResult> GetByEmployee(string employeeId)
    {
        var result = await _mediator.Send(new GetAllRequestsQuery(employeeId));
        return Ok(result);
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        //  Pending requests query 
        var result = await _mediator.Send(new GetPendingRequestsQuery()); 
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool isDivisionHead)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var result = await _mediator.Send(new GetAllRequestsQuery(userId, isDivisionHead));
        return Ok(result);
    }
    
}