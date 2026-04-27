using Assura.Application.Common.Interfaces;

using MediatR;

using Microsoft.EntityFrameworkCore;

using System.Linq;

using System.Threading;

using System.Threading.Tasks;



namespace Assura.Application.Features.Notifications.Commands;



public record MarkAllNotificationsAsReadCommand(int UserId) : IRequest;



public class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommand>

{

    private readonly IApplicationDbContext _context;



    public MarkAllNotificationsAsReadCommandHandler(IApplicationDbContext context)

    {

        _context = context;

    }



    public async Task Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)

    {

        var notifications = await _context.Notifications

            .Where(n => n.UserId == request.UserId && !n.IsRead)

            .ToListAsync(cancellationToken);



        foreach (var notification in notifications)

        {

            notification.IsRead = true;

        }



        if (notifications.Any())

        {

            await _context.SaveChangesAsync(cancellationToken);

        }

    }

}

