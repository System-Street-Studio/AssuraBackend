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
            .FirstOrDefaultAsync(m => m.Id == request.MaintenanceId, cancellationToken)
            ?? throw new Exception($"Maintenance {request.MaintenanceId} not found");

        maintenance.Status = "EscalatedToProcurement";
        maintenance.EscalatedToProcurementAt = DateTime.UtcNow;
        maintenance.StorekeeperUserId = request.StorekeeperUserId;
        if (!string.IsNullOrEmpty(request.Notes))
            maintenance.Notes = request.Notes;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("[Maintenance] {Id} escalated to procurement by storekeeper {UserId}",
            request.MaintenanceId, request.StorekeeperUserId);
    }
}
