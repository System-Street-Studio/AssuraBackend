using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using MediatR;

namespace Assura.Application.Features.AssetRequests.Commands;

public enum CancelAssetRequestResult
{
    Success,
    NotFound,
    Forbidden,
    InvalidStatus
}

public record CancelAssetRequestCommand(int Id, int UserId, bool IsPrivileged) : IRequest<CancelAssetRequestResult>;

public class CancelAssetRequestHandler : IRequestHandler<CancelAssetRequestCommand, CancelAssetRequestResult>
{
    private readonly IApplicationDbContext _context;

    public CancelAssetRequestHandler(IApplicationDbContext context) => _context = context;

    public async Task<CancelAssetRequestResult> Handle(CancelAssetRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.AssetRequests.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null) return CancelAssetRequestResult.NotFound;

        if (!request.IsPrivileged && entity.RequesterId != request.UserId.ToString())
        {
            return CancelAssetRequestResult.Forbidden;
        }

        // Only a still-pending request can be withdrawn — once a Storekeeper/Division
        // Head has started acting on it, cancelling out from under them would leave
        // their workflow state inconsistent.
        if (entity.Status != RequestStatus.Pending)
        {
            return CancelAssetRequestResult.InvalidStatus;
        }

        entity.Status = RequestStatus.Cancelled;
        await _context.SaveChangesAsync(cancellationToken);

        return CancelAssetRequestResult.Success;
    }
}
