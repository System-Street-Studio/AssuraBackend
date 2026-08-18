using Assura.Application.Features.Notifications.Queries;
using Assura.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Assura.Application.Features.Notifications.Commands;

namespace Assura.API.Controllers;

[Authorize]
public class NotificationsController : BaseApiController
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<NotificationDto>>> GetNotifications()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();
        
        return await _mediator.Send(new GetNotificationsQuery(userId));
    }

    [HttpPost("{id}/mark-as-read")]
    public async Task<ActionResult> MarkAsRead(int id)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

        await _mediator.Send(new MarkNotificationAsReadCommand(id, userId));
        return NoContent();
    }

    [HttpPost("mark-all-as-read")]
    public async Task<ActionResult> MarkAllAsRead()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

        await _mediator.Send(new MarkAllNotificationsAsReadCommand(userId));
        return NoContent();
    }
}
