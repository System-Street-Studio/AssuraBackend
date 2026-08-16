using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Assura.Application.Features.Maintenances.Commands;

public record UpdateMaintenanceStatusCommand(int MaintenanceId, string NewStatus, int UserId, bool IsDivisionHead = false) : IRequest;

public class UpdateMaintenanceStatusCommandHandler : IRequestHandler<UpdateMaintenanceStatusCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateMaintenanceStatusCommandHandler> _logger;

    public UpdateMaintenanceStatusCommandHandler(IApplicationDbContext context, ILogger<UpdateMaintenanceStatusCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(UpdateMaintenanceStatusCommand request, CancellationToken cancellationToken)
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
            var caller = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            if (caller?.DivisionId == null || maintenance.Asset.DivisionId == null || caller.DivisionId != maintenance.Asset.DivisionId)
            {
                throw new UnauthorizedAccessException("Division Head may only act on maintenance records within their own division.");
            }
        }

        maintenance.Status = request.NewStatus;

        switch (request.NewStatus)
        {
            case "Approved":
                maintenance.ApprovedByUserId = request.UserId;
                maintenance.ApprovedAt = DateTime.UtcNow;
                break;
            case "InProgress":
                maintenance.StorekeeperUserId = request.UserId;
                maintenance.StartedAt = DateTime.UtcNow;
                break;
            case "Completed":
                maintenance.CompletedAt = DateTime.UtcNow;
                break;
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("[Maintenance] Updated {Id} to {Status} by user {UserId}",
            request.MaintenanceId, request.NewStatus, request.UserId);
    }
}
