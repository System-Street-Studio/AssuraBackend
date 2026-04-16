using Assura.Application.Common.Interfaces;
using Assura.Domain.Constants;
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
    public int? ProcessedByUserId { get; init; }
}

public class ProcessRequestCommandHandler : IRequestHandler<ProcessRequestCommand>
{
    private readonly IApplicationDbContext _context;

    public ProcessRequestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ProcessRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Requests
            .Include(r => r.Requester)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (entity == null) return;

        entity.Remarks = request.Remarks;
        entity.StorekeeperProcessorId = request.ProcessedByUserId;
        entity.StorekeeperProcessedAt = DateTime.UtcNow;

        if (request.IsInStock)
        {
            if (!request.AssetId.HasValue)
            {
                return;
            }

            // In stock means reserve temporarily and wait for physical handover confirmation.
            entity.Status = RequestWorkflowStatus.TemporaryAssigned;
            entity.AssetId = request.AssetId;
            entity.TemporarilyAssignedAt = DateTime.UtcNow;

            var asset = await _context.Assets
                .FirstOrDefaultAsync(a => a.Id == request.AssetId.Value, cancellationToken);

            if (asset != null)
            {
                asset.ReservedForUserId = entity.RequesterId;
                asset.ReservedByRequestId = entity.Id;
                asset.ReservedUntilUtc = DateTime.UtcNow.AddHours(48);
            }

            // Notify employee that reserved asset is ready for pickup.
            _context.Notifications.Add(new Notification
            {
                Title = "Asset Reserved for Pickup",
                Message = $"Your request {entity.RequestNumber} has a temporary reserved asset. Collect it from stores for final confirmation.",
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
                        Title = "Temporary Asset Assigned",
                        Message = $"Request {entity.RequestNumber} has a temporary asset reservation and is awaiting pickup confirmation.",
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
            entity.Status = RequestWorkflowStatus.PendingProcurement;

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
    }
}
