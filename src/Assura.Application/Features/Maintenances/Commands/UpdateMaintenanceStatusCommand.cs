using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Assura.Application.Features.Maintenances.Commands;

public record UpdateMaintenanceStatusCommand(int MaintenanceId, string NewStatus, int UserId) : IRequest;

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
            .FirstOrDefaultAsync(m => m.Id == request.MaintenanceId, cancellationToken)
            ?? throw new Exception($"Maintenance {request.MaintenanceId} not found");

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
