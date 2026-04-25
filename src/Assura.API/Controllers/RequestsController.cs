using Assura.Application.Features.Requests.Commands;
using Assura.Application.Features.Requests.Queries;
using Assura.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Assura.Domain.Enums;

namespace Assura.API.Controllers;

[Authorize]
public class RequestsController : BaseApiController
{
    private readonly IMediator _mediator;

    public RequestsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<RequestDto>>> GetRequests()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var roleStr = User.FindFirstValue(ClaimTypes.Role);
        
        int? userId = int.TryParse(userIdStr, out var id) ? id : 1; // Fallback to 1 for testing
        UserRole? role = Enum.TryParse<UserRole>(roleStr, out var r) ? r : UserRole.Employee; // Fallback to Employee if role missing

        return await _mediator.Send(new GetRequestsQuery(userId, role));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RequestDto>> GetRequestById(int id)
    {
        if (id <= 0) return BadRequest();

        var request = await _mediator.Send(new GetRequestByIdQuery(id));
        if (request == null) return NotFound();

        return request;
    }

    [HttpGet("{id}/suggested-assets")]
    [Authorize(Roles = "Storekeeper,Admin")]
    public async Task<ActionResult<List<SuggestedAssetDto>>> GetSuggestedAssets(int id)
    {
        if (id <= 0) return BadRequest();
        return await _mediator.Send(new GetSuggestedAssetsForRequestQuery(id));
    }

    [HttpPost]
    public async Task<ActionResult<int>> CreateRequest([FromBody] CreateRequestCommand command)
    {
        // Force requester ID from token for security
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdStr, out var userId))
        {
            var finalCommand = command with { RequesterId = userId };
            return await _mediator.Send(finalCommand);
        }
        
        return Unauthorized();
    }

    [HttpPost("{id}/process")]
    [Authorize(Roles = "Storekeeper,Admin,Procurement")]
    public async Task<ActionResult> ProcessRequest(int id, [FromBody] ProcessRequestCommand command)
    {
        if (id != command.Id) return BadRequest();
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var finalCommand = int.TryParse(userIdStr, out var userId)
            ? command with { ProcessedByUserId = userId }
            : command;

        await _mediator.Send(finalCommand);
        return NoContent();
    }

    [HttpPost("{id}/division-head-review")]
    [Authorize(Roles = "DivisionHead,Admin")]
    public async Task<ActionResult> ReviewByDivisionHead(int id, [FromBody] ReviewRequestByDivisionHeadCommand command)
    {
        if (id != command.Id) return BadRequest();

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var finalCommand = int.TryParse(userIdStr, out var userId)
            ? command with { ReviewedByUserId = userId }
            : command;

        await _mediator.Send(finalCommand);
        return NoContent();
    }

    [HttpPost("{id}/confirm-temporary-assignment")]
    [Authorize(Roles = "Storekeeper,Admin")]
    public async Task<ActionResult> ConfirmTemporaryAssignment(int id, [FromBody] ConfirmTemporaryAssignmentCommand command)
    {
        if (id != command.Id) return BadRequest();

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var finalCommand = int.TryParse(userIdStr, out var userId)
            ? command with { ConfirmedByUserId = userId }
            : command;

        await _mediator.Send(finalCommand);
        return NoContent();
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult> UpdateStatus(int id, [FromBody] UpdateRequestStatusCommand command)
    {
        if (id != command.Id) return BadRequest();
        await _mediator.Send(command);
        return NoContent();
    }
}
