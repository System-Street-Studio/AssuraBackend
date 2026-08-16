using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Assura.Application.Features.Maintenances.Commands;

public enum InformMaintenanceStakeholdersResult
{
    Success,
    NotFound,
    InvalidStatus
}

public record InformMaintenanceStakeholdersCommand : IRequest<InformMaintenanceStakeholdersResult>
{
    public int MaintenanceId { get; init; }
    public int StorekeeperUserId { get; init; }
}

public class InformMaintenanceStakeholdersCommandHandler : IRequestHandler<InformMaintenanceStakeholdersCommand, InformMaintenanceStakeholdersResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<InformMaintenanceStakeholdersCommandHandler> _logger;

    public InformMaintenanceStakeholdersCommandHandler(IApplicationDbContext context, ILogger<InformMaintenanceStakeholdersCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<InformMaintenanceStakeholdersResult> Handle(InformMaintenanceStakeholdersCommand request, CancellationToken cancellationToken)
    {
        var maintenance = await _context.Maintenances
            .Include(m => m.Asset)
            .Include(m => m.RequestedByUser)
            .FirstOrDefaultAsync(m => m.Id == request.MaintenanceId, cancellationToken);

        if (maintenance == null)
        {
            return InformMaintenanceStakeholdersResult.NotFound;
        }

        // Only a Procurement-completed maintenance can be reported back to the
        // employee/division head — otherwise the Storekeeper could tell them work
        // is done before it actually is.
        if (!string.Equals(maintenance.Status, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            return InformMaintenanceStakeholdersResult.InvalidStatus;
        }

        var assetLabel = maintenance.Asset != null ? maintenance.Asset.AssetCode : $"Asset #{maintenance.AssetId}";

        if (maintenance.RequestedByUserId.HasValue)
        {
            _context.Notifications.Add(new Notification
            {
                Title = "Maintenance Completed",
                Message = $"Maintenance for '{assetLabel}' ({maintenance.MaintenanceNumber}) has been completed and the asset is ready.",
                UserId = maintenance.RequestedByUserId.Value,
                Type = "Success",
                ReferenceId = maintenance.Id.ToString()
            });

            var employeeDivisionId = maintenance.RequestedByUser?.DivisionId;
            if (employeeDivisionId.HasValue)
            {
                var divisionHeads = await _context.Users
                    .Where(u => u.Role == UserRole.DivisionHead && u.DivisionId == employeeDivisionId.Value)
                    .ToListAsync(cancellationToken);

                foreach (var head in divisionHeads)
                {
                    _context.Notifications.Add(new Notification
                    {
                        Title = "Maintenance Completed",
                        Message = $"Maintenance for '{assetLabel}' ({maintenance.MaintenanceNumber}), requested by {maintenance.RequestedByUser!.FirstName} {maintenance.RequestedByUser.LastName}, has been completed.",
                        UserId = head.Id,
                        Type = "Success",
                        ReferenceId = maintenance.Id.ToString()
                    });
                }
            }
        }

        maintenance.Status = "Submitted";
        maintenance.StorekeeperUserId = request.StorekeeperUserId;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("[Maintenance] {Id} stakeholders informed and submitted by storekeeper {UserId}",
            request.MaintenanceId, request.StorekeeperUserId);

        return InformMaintenanceStakeholdersResult.Success;
    }
}
