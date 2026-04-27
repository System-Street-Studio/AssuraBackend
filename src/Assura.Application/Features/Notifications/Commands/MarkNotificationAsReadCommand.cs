using Assura.Domain.Interfaces;

using MediatR;

using System.Threading;

using System.Threading.Tasks;



namespace Assura.Application.Features.Notifications.Commands;



public record MarkNotificationAsReadCommand(int Id) : IRequest;



public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand>

{

    private readonly IApplicationDbContext _context;



    public MarkNotificationAsReadCommandHandler(IApplicationDbContext context)

    {

        _context = context;

    }



    public async Task Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)

    {

        var entity = await _context.Notifications.FindAsync(new object[] { request.Id }, cancellationToken);



        if (entity != null)

        {

            entity.IsRead = true;

            await _context.SaveChangesAsync(cancellationToken);

        }

    }

}

