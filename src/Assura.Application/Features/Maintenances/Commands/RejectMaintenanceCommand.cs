using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Assura.Application.Features.Maintenances.Commands;

public record RejectMaintenanceCommand : IRequest
{
    public int MaintenanceId { get; init; }
    public int RejectedByUserId { get; init; }
    public string? Reason { get; init; }
    public bool IsDivisionHead { get; init; }
}

public class RejectMaintenanceCommandHandler : IRequestHandler<RejectMaintenanceCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<RejectMaintenanceCommandHandler> _logger;

    public RejectMaintenanceCommandHandler(IApplicationDbContext context, ILogger<RejectMaintenanceCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(RejectMaintenanceCommand request, CancellationToken cancellationToken)
    {
        var maintenance = await _context.Maintenances
            .Include(m => m.Asset)
            .FirstOrDefaultAsync(m => m.Id == request.MaintenanceId, cancellationToken)
            ?? throw new Exception($"Maintenance {request.MaintenanceId} not found");

        // Division Heads may only act on maintenance records for assets in their own
        // division; Admin/Procurement/Storekeeper/Maintenance roles remain fully
        // privileged, matching the scoping pattern already used for asset requests
        // (ApproveAssetRequestCommand).
        if (request.IsDivisionHead)
        {
            var caller = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.RejectedByUserId, cancellationToken);
            if (caller?.DivisionId == null || maintenance.Asset.DivisionId == null || caller.DivisionId != maintenance.Asset.DivisionId)
            {
                throw new UnauthorizedAccessException("Division Head may only act on maintenance records within their own division.");
            }
        }

        maintenance.Status = "Rejected";
        maintenance.Notes = request.Reason;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("[Maintenance] {Id} rejected by user {UserId}",
            request.MaintenanceId, request.RejectedByUserId);
    }
}
