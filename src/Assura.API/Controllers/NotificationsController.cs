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

[AllowAnonymous]
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
        int userId;
        
        if (int.TryParse(userIdStr, out var id))
        {
            userId = id;
        }
        else
        {
            // Fallback for testing
            userId = 1;
        }
        
        return await _mediator.Send(new GetNotificationsQuery(userId));
    }

    [HttpPost("{id}/mark-as-read")]
    public async Task<ActionResult> MarkAsRead(int id)
    {
        await _mediator.Send(new MarkNotificationAsReadCommand(id));
        return NoContent();
    }

    [HttpPost("mark-all-as-read")]
    public async Task<ActionResult> MarkAllAsRead()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int userId;
        
        if (int.TryParse(userIdStr, out var id))
        {
            userId = id;
        }
        else
        {
            // Fallback for testing
            userId = 1;
        }

        await _mediator.Send(new MarkAllNotificationsAsReadCommand(userId));
        return NoContent();
    }
}
