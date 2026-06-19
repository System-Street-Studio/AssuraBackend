using Assura.Application.Common.Interfaces;

using Assura.Application.DTOs;

using MediatR;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;

using System.Linq;

using System.Threading;

using System.Threading.Tasks;



namespace Assura.Application.Features.Notifications.Queries;



public record GetNotificationsQuery(int UserId) : IRequest<List<NotificationDto>>;



public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, List<NotificationDto>>

{

    private readonly IApplicationDbContext _context;



    public GetNotificationsQueryHandler(IApplicationDbContext context)

    {

        _context = context;

    }



    public async Task<List<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)

    {

        var notifications = await _context.Notifications

            .Where(n => n.UserId == request.UserId)

            .OrderByDescending(n => n.CreatedAt)

            .ToListAsync(cancellationToken);



        return notifications.Select(n => new NotificationDto

        {

            Id = n.Id,

            Title = n.Title,

            Message = n.Message,

            IsRead = n.IsRead,

            Type = n.Type,

            ReferenceId = n.ReferenceId,

            CreatedAt = n.CreatedAt,

            Icon = GetIconForType(n.Type, n.Title)

        }).ToList();

    }



    private string GetIconForType(string? type, string title)

    {

        if (title.Contains("Asset Request", StringComparison.OrdinalIgnoreCase)) return "swap_horiz";

        if (title.Contains("Item Received", StringComparison.OrdinalIgnoreCase)) return "inventory_2";

        if (title.Contains("Assigned", StringComparison.OrdinalIgnoreCase)) return "exit_to_app";

        if (title.Contains("Warranty", StringComparison.OrdinalIgnoreCase)) return "schedule";

        

        return type switch

        {

            "Success" => "check_circle",

            "Warning" => "warning",

            "Error" => "error",

            _ => "info"

        };

    }

}

