using MediatR;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using Assura.Application.Features.AssetRequests.Events;
using Assura.Domain.Entities;
using System.Text.Json;

namespace Assura.Application.Features.AssetRequests.Commands;

public record ApproveAssetRequestCommand(int Id) : IRequest<bool>;

public class ApproveAssetRequestHandler : IRequestHandler<ApproveAssetRequestCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IPublisher _publisher;
    public ApproveAssetRequestHandler(IApplicationDbContext context, IPublisher publisher)
    {
        
     _context = context;
     _publisher = publisher;
    }

    public async Task<bool> Handle(ApproveAssetRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.AssetRequests.FindAsync(new object[] { request.Id }, cancellationToken);
        
        if (entity == null) return false;

        entity.Status = RequestStatus.Approved; // status  change 
        await _context.SaveChangesAsync(cancellationToken);

        // Notify requester
        if (int.TryParse(entity.RequesterId, out var requesterId))
        {
            _context.Notifications.Add(new Notification
            {
                Title = "Asset Request Approved",
                Message = $"Your asset request ({entity.AssetName}) has been approved.",
                UserId = requesterId,
                Type = "Success",
                ReferenceId = entity.Id.ToString()
            });
            await _context.SaveChangesAsync(cancellationToken);
        }

        await _publisher.Publish(new AssetRequestApprovedEvent(
            entity.Id ,  
            entity.AssetName,
            entity.AssetCategory,
            entity.Quantity ?? 0,
            entity.RequestType,
            entity.Priority,
            entity.Status .ToString(),
            entity.RequesterName,
            entity.RequesterId,
            entity.Attachments?.Count > 0 
                ? JsonSerializer.Serialize(entity.Attachments.Select(a => new { a.FileName, a.FileUrl }).ToList())
                : "N/A",
            entity.SubmittedDate,
            entity.Description ?? "N/A",
            entity.Reason ?? "N/A"
        ), cancellationToken);

        return true;
    }
}