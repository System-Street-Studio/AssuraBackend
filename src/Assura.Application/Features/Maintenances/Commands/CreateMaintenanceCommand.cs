using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Assura.Application.Features.Maintenances.Commands;

public record CreateMaintenanceCommand : IRequest<int>
{
    public string MaintenanceNumber { get; set; } = string.Empty;
    public MaintenanceType Type { get; set; }
    public DateTime MaintenanceDate { get; set; }
    public string? Description { get; set; }
    public decimal Cost { get; set; }
    public string? Status { get; set; }
    public int AssetId { get; set; }
    public int? RepairingFirmId { get; set; }
}

public class CreateMaintenanceCommandHandler : IRequestHandler<CreateMaintenanceCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateMaintenanceCommandHandler> _logger;

    public CreateMaintenanceCommandHandler(IApplicationDbContext context, ILogger<CreateMaintenanceCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<int> Handle(CreateMaintenanceCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[DEBUG] CreateMaintenanceCommandHandler: Creating new maintenance record: {@Request}", request);

        var maintenance = new Maintenance
        {
            MaintenanceNumber = request.MaintenanceNumber,
            Type = request.Type,
            MaintenanceDate = request.MaintenanceDate,
            Description = request.Description,
            Cost = request.Cost,
            Status = request.Status,
            AssetId = request.AssetId,
            RepairingFirmId = request.RepairingFirmId
        };

        _context.Maintenances.Add(maintenance);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("[DEBUG] CreateMaintenanceCommandHandler: Created record with ID {Id}", maintenance.Id);
        return maintenance.Id;
    }
}
