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
        
        int? userId = int.TryParse(userIdStr, out var id) ? id : null;
        UserRole? role = Enum.TryParse<UserRole>(roleStr, out var r) ? r : null;

        return await _mediator.Send(new GetRequestsQuery(userId, role));
    }

    [HttpPost]
    public async Task<ActionResult<int>> CreateRequest([FromBody] CreateRequestCommand command)
    {
        // Force requester ID from token for security
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdStr, out var userId))
        {
            // We use a modified command or handle it in handler, for now let's assume client sends it or we override
            var finalCommand = command with { RequesterId = userId };
            return await _mediator.Send(finalCommand);
        }
        return BadRequest("Cannot identify user");
    }
}
