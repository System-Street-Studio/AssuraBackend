using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Assura.Domain.Entities;

namespace Assura.Application.Features.Requests.Commands;

public record ProcessRequestCommand : IRequest
{
    public int Id { get; init; }
    public int? AssetId { get; init; }
    public bool IsInStock { get; init; }
    public string? Remarks { get; init; }
}

public class ProcessRequestCommandHandler : IRequestHandler<ProcessRequestCommand>
{
    private readonly IApplicationDbContext _context;

    public ProcessRequestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(ProcessRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Requests
            .Include(r => r.Requester)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (entity == null) return Unit.Value;

        entity.Remarks = request.Remarks;

        if (request.IsInStock)
        {
            // Flow: Found -> Notify Employee & Division Head
            entity.Status = "Approved";
            entity.AssetId = request.AssetId;

            // 1. Notify Requester
            _context.Notifications.Add(new Notification
            {
                Title = "Request Approved",
                Message = $"Your request {entity.RequestNumber} has been approved and an asset has been allocated.",
                UserId = entity.RequesterId,
                Type = "Success",
                ReferenceId = entity.Id.ToString()
            });

            // 2. Notify Division Head
            if (entity.Requester.DivisionId.HasValue)
            {
                var divisionHeads = await _context.Users
                    .Where(u => u.DivisionId == entity.Requester.DivisionId && u.Role == UserRole.DivisionHead)
                    .ToListAsync(cancellationToken);

                foreach (var head in divisionHeads)
                {
                    _context.Notifications.Add(new Notification
                    {
                        Title = "Asset Allocated in Division",
                        Message = $"An asset has been allocated for {entity.Requester.FirstName} {entity.Requester.LastName}'s request ({entity.RequestNumber}).",
                        UserId = head.Id,
                        Type = "Info",
                        ReferenceId = entity.Id.ToString()
                    });
                }
            }
        }
        else
        {
            // Flow: Not Found -> Notify Procurement
            entity.Status = "PendingProcurement";

            var procurementUsers = await _context.Users
                .Where(u => u.Role == UserRole.Procurement || u.Role == UserRole.Admin)
                .ToListAsync(cancellationToken);

            foreach (var user in procurementUsers)
            {
                _context.Notifications.Add(new Notification
                {
                    Title = "Asset Escalated to Procurement",
                    Message = $"Request {entity.RequestNumber} could not be fulfilled from stock and requires procurement.",
                    UserId = user.Id,
                    Type = "Warning",
                    ReferenceId = entity.Id.ToString()
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
