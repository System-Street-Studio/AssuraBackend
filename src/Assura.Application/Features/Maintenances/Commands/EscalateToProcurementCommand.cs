using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Assura.Application.Features.Maintenances.Commands;

public record EscalateToProcurementCommand : IRequest
{
    public int MaintenanceId { get; init; }
    public int StorekeeperUserId { get; init; }
    public string? Notes { get; init; }
    public bool IsDivisionHead { get; init; }
}

public class EscalateToProcurementCommandHandler : IRequestHandler<EscalateToProcurementCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<EscalateToProcurementCommandHandler> _logger;

    public EscalateToProcurementCommandHandler(IApplicationDbContext context, ILogger<EscalateToProcurementCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(EscalateToProcurementCommand request, CancellationToken cancellationToken)
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
            var caller = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.StorekeeperUserId, cancellationToken);
            if (caller?.DivisionId == null || maintenance.Asset.DivisionId == null || caller.DivisionId != maintenance.Asset.DivisionId)
            {
                throw new UnauthorizedAccessException("Division Head may only act on maintenance records within their own division.");
            }
        }

        maintenance.Status = "EscalatedToProcurement";
        maintenance.EscalatedToProcurementAt = DateTime.UtcNow;
        maintenance.StorekeeperUserId = request.StorekeeperUserId;
        if (!string.IsNullOrEmpty(request.Notes))
            maintenance.Notes = request.Notes;

        if (maintenance.OriginalRequestId.HasValue)
        {
            bool isAssetRequest = maintenance.MaintenanceNumber != null && maintenance.MaintenanceNumber.Contains("-AR");

            if (isAssetRequest)
            {
                var originalAssetRequest = await _context.AssetRequests.FindAsync(new object[] { maintenance.OriginalRequestId.Value }, cancellationToken);
                if (originalAssetRequest != null)
                {
                    originalAssetRequest.Status = Assura.Domain.Enums.RequestStatus.PendingProcurement;
                }
            }
            else
            {
                var originalRequest = await _context.Requests.FindAsync(new object[] { maintenance.OriginalRequestId.Value }, cancellationToken);
                if (originalRequest != null)
                {
                    originalRequest.Status = "PendingProcurement";
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("[Maintenance] {Id} escalated to procurement by storekeeper {UserId}",
            request.MaintenanceId, request.StorekeeperUserId);
    }
}
