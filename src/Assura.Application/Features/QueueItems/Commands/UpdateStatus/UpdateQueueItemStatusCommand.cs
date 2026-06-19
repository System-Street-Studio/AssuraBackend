using MediatR;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;

namespace Assura.Application.Features.QueueItems.Commands.UpdateStatus;

public class UpdateQueueItemStatusCommand : IRequest<bool>
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ReviewNote { get; set; }
}

public class UpdateQueueItemStatusCommandHandler : IRequestHandler<UpdateQueueItemStatusCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateQueueItemStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateQueueItemStatusCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.QueueItems.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null) return false;

        if (Enum.TryParse<QueueItemStatus>(request.Status, true, out var status))
        {
            entity.Status = status;
        }

        if (!string.IsNullOrEmpty(request.ReviewNote))
        {
            entity.ReviewNote = request.ReviewNote;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
