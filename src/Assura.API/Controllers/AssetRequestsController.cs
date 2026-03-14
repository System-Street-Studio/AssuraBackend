using Microsoft.AspNetCore.Mvc;
using MediatR;
using Assura.Application.Features.AssetRequests.Commands;
using Assura.Domain.Entities;
using Assura.Application.Features.AssetRequests.Queries;

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

    [HttpPatch("{id}/approve")]
    public async Task<ActionResult> Approve(int id)
    {
        var result = await _mediator.Send(new ApproveAssetRequestCommand(id));
        return result ? NoContent() : NotFound();
    }

    [HttpGet("employee/{employeeId}")] // මේ පේළිය අනිවාර්යයි
    public async Task<IActionResult> GetByEmployee(string employeeId)
    {
        var result = await _mediator.Send(new GetAllRequestsQuery(employeeId));
        return Ok(result);
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        // මෙතනට Pending ඉල්ලීම් ගන්න query එකක් හදන්න ඕනේ
        var result = await _mediator.Send(new GetPendingRequestsQuery()); 
        return Ok(result);
    }

    
}