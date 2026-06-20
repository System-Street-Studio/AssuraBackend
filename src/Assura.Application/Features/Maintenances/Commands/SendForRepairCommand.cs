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
