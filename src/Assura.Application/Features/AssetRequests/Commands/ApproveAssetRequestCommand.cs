using MediatR;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;

namespace Assura.Application.Features.AssetRequests.Commands;

public record ApproveAssetRequestCommand(int Id) : IRequest<bool>;

public class ApproveAssetRequestHandler : IRequestHandler<ApproveAssetRequestCommand, bool>
{
    private readonly IApplicationDbContext _context;
    public ApproveAssetRequestHandler(IApplicationDbContext context) => _context = context;

    public async Task<bool> Handle(ApproveAssetRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.AssetRequests.FindAsync(new object[] { request.Id }, cancellationToken);
        
        if (entity == null) return false;

        entity.Status = RequestStatus.Approved; // මෙන්න මෙතනයි status එක change වෙන්නේ
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}