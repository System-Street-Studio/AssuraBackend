using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Assets.Commands;

public record VerifyAssetCommand(int AssetId) : IRequest<bool>;

public class VerifyAssetCommandHandler : IRequestHandler<VerifyAssetCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public VerifyAssetCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(VerifyAssetCommand request, CancellationToken cancellationToken)
    {
        var asset = await _context.Assets
            .FirstOrDefaultAsync(a => a.Id == request.AssetId, cancellationToken);

        if (asset == null)
        {
            return false;
        }

        int? currentUserId = null;
        if (int.TryParse(_currentUserService.UserId, out int parsedId))
        {
            currentUserId = parsedId;
        }

        asset.LastVerifiedAt = DateTime.UtcNow;
        asset.LastVerifiedByUserId = currentUserId;

        // Add Audit Log
        var auditLog = new AuditLog
        {
            Action = "Verify",
            EntityName = "Asset",
            EntityId = asset.AssetCode,
            IpAddress = "N/A",
            NewValues = $"Verified by {_currentUserService.UserId ?? "Unknown"}"
        };

        _context.AuditLogs.Add(auditLog);

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
