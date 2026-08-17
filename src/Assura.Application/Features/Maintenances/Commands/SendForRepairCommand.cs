using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Assura.Application.Features.Maintenances.Commands;

public record SendForRepairCommand : IRequest
{
    public int MaintenanceId { get; init; }
    public int? RepairingFirmId { get; init; }
    public int StorekeeperUserId { get; init; }
    public string? Notes { get; init; }
    public bool IsDivisionHead { get; init; }
}

public class SendForRepairCommandHandler : IRequestHandler<SendForRepairCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<SendForRepairCommandHandler> _logger;

    public SendForRepairCommandHandler(IApplicationDbContext context, ILogger<SendForRepairCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(SendForRepairCommand request, CancellationToken cancellationToken)
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

        maintenance.Asset.Status = AssetStatus.UnderMaintenance;
        maintenance.Status = "SentForRepair";
        maintenance.RepairingFirmId = request.RepairingFirmId;
        maintenance.StorekeeperUserId = request.StorekeeperUserId;
        maintenance.StartedAt ??= DateTime.UtcNow;
        if (!string.IsNullOrEmpty(request.Notes))
            maintenance.Notes = request.Notes;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("[Maintenance] {Id} sent for repair by storekeeper {UserId}",
            request.MaintenanceId, request.StorekeeperUserId);
    }
}
