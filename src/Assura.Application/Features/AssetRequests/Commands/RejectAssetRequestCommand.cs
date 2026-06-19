using MediatR;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using Assura.Domain.Entities;

namespace Assura.Application.Features.AssetRequests.Commands;
//public record RejectAssetRequestCommand(int Id, string Reason) : IRequest<bool>;
public record RejectAssetRequestCommand(int Id) : IRequest<bool>;

public class RejectAssetRequestHandler : IRequestHandler<RejectAssetRequestCommand, bool>
{
    private readonly IApplicationDbContext _context;
    public RejectAssetRequestHandler(IApplicationDbContext context) => _context = context;

    public async Task<bool> Handle(RejectAssetRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.AssetRequests.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null) return false;

        entity.Status = RequestStatus.Rejected;
        await _context.SaveChangesAsync(cancellationToken);

        // Notify requester
        if (int.TryParse(entity.RequesterId, out var requesterId))
        {
            _context.Notifications.Add(new Notification
            {
                Title = "Asset Request Rejected",
                Message = $"Your asset request ({entity.AssetName}) has been rejected.",
                UserId = requesterId,
                Type = "Error",
                ReferenceId = entity.Id.ToString()
            });
            await _context.SaveChangesAsync(cancellationToken);
        }
        return true;
    }
}