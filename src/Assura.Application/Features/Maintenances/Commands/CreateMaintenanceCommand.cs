using Assura.Application.Common.Interfaces;
using Assura.Domain.Constants;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

    // Id of the pending-procurement queue item (Request or AssetRequest) this note
    // was created from, if any — used to clear that item out of the Procurement
    // queue once a note has been raised for it. Optional because notes can also be
    // created ad hoc, with no originating request.
    public int? RequestId { get; set; }
}

public class CreateMaintenanceCommandValidator : AbstractValidator<CreateMaintenanceCommand>
{
    public CreateMaintenanceCommandValidator()
    {
        RuleFor(x => x.RepairingFirmId)
            .NotNull()
            .WithMessage("Repair Firm is required.");
    }
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

        // Clear the originating queue item so it stops showing up in Procurement's
        // pending-requests queue (GetPendingAssetRequestsQuery only returns items
        // still in the PendingProcurement status) now that a note has been raised
        // for it. The queue combines two different tables with unqualified ids, so
        // try the `Requests` table first, then fall back to `AssetRequests`.
        if (request.RequestId.HasValue)
        {
            if (request.RequestId.Value < 0)
            {
                var actualId = Math.Abs(request.RequestId.Value);
                var originalAssetRequest = await _context.AssetRequests
                    .FirstOrDefaultAsync(ar => ar.Id == actualId, cancellationToken);

                if (originalAssetRequest != null && originalAssetRequest.Status == RequestStatus.PendingProcurement)
                {
                    // RequestStatus.Passed is otherwise unused; repurposed here to mean
                    // "resolved via a Procurement-created Maintenance note".
                    originalAssetRequest.Status = RequestStatus.Passed;
                }
            }
            else
            {
                var originalRequest = await _context.Requests
                    .FirstOrDefaultAsync(r => r.Id == request.RequestId.Value, cancellationToken);

                if (originalRequest != null && originalRequest.Status == RequestWorkflowStatus.PendingProcurement)
                {
                    originalRequest.Status = "Completed";
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("[DEBUG] CreateMaintenanceCommandHandler: Created record with ID {Id}", maintenance.Id);
        return maintenance.Id;
    }
}
