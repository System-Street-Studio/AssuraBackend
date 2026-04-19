using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Requests.Commands;

public record UpdateRequestStatusCommand : IRequest
{
    public int Id { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Notes { get; init; }
}

public class UpdateRequestStatusCommandHandler : IRequestHandler<UpdateRequestStatusCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateRequestStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateRequestStatusCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Requests
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (entity == null) return;

        entity.Status = request.Status;
        if (!string.IsNullOrEmpty(request.Notes))
        {
            entity.Remarks = request.Notes;
        }

        // Add a notification for the requester
        _context.Notifications.Add(new Notification
        {
            Title = $"Request {request.Status}",
            Message = $"Your request {entity.RequestNumber} has been {request.Status.ToLower()}.",
            UserId = entity.RequesterId,
            Type = request.Status == "Approved" || request.Status == "Fulfilled" ? "Success" : "Error",
            ReferenceId = entity.Id.ToString()
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
