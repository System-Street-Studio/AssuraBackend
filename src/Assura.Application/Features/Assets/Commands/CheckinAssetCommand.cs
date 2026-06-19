using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Assura.Application.Features.Assets.Commands;

/// <summary>
/// Command to process the check-in (return) of an asset from an employee.
/// Captures the asset's condition, severity of any damage, and whether repair is needed.
/// </summary>
public record CheckinAssetCommand(
    int Id,
    string Condition,
    string? Notes,
    string? CheckedInBy,
    string? DamageSeverity,
    bool RepairNeeded,
    bool Acknowledged,
    string? EvidenceFileName) : IRequest<AssetDto?>;

public class CheckinAssetCommandValidator : AbstractValidator<CheckinAssetCommand>
{
    public CheckinAssetCommandValidator()
    {
        RuleFor(x => x.Condition)
            .NotEmpty();

        RuleFor(x => x.Acknowledged)
            .Equal(true)
            .WithMessage("Check-in acknowledgement is required.");

        RuleFor(x => x.DamageSeverity)
            .NotEmpty()
            .When(x => x.Condition == "Damaged" || x.RepairNeeded)
            .WithMessage("Damage severity is required for damaged/repair-needed check-ins.");
    }
}

internal class CheckinCheckoutRecordMeta
{
    public DateOnly? DueDate { get; set; }
    public string? Condition { get; set; }
    public string? DamageSeverity { get; set; }
    public bool RepairNeeded { get; set; }
    public bool Acknowledged { get; set; }
    public string? EvidenceFileName { get; set; }
    public string? MaintenanceNumber { get; set; }
    public string? CheckedOutBy { get; set; }
    public string? CheckedInBy { get; set; }
    public string? CheckinNotes { get; set; }
}

/// <summary>
/// Handler for executing the <see cref="CheckinAssetCommand"/>.
/// Updates the asset status (either InStore or UnderMaintenance), updates the checkout request,
/// and automatically creates a Maintenance record if the asset is damaged.
/// </summary>
public class CheckinAssetCommandHandler : IRequestHandler<CheckinAssetCommand, AssetDto?>
{
    private readonly IApplicationDbContext _context;

    public CheckinAssetCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AssetDto?> Handle(CheckinAssetCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Assets
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (entity == null) return null;

        var requiresMaintenance = request.Condition == "Damaged" || request.RepairNeeded;

        // Condition-based status update
        entity.Status = requiresMaintenance ? AssetStatus.UnderMaintenance : AssetStatus.InStore;
        entity.AssignedUserId = null;
        
        if (!string.IsNullOrEmpty(request.Notes))
        {
            entity.Notes = string.IsNullOrEmpty(entity.Notes) 
                ? $"Check-in: {request.Notes}" 
                : $"{entity.Notes} | Check-in: {request.Notes}";
        }

        var checkoutRequest = await _context.Requests
            .Where(r => r.Type == RequestType.Asset && r.AssetId == request.Id && r.Status == "Checked Out")
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (checkoutRequest != null)
        {
            var meta = ParseMeta(checkoutRequest.Remarks);
            meta.Condition = request.Condition;
            meta.DamageSeverity = request.DamageSeverity;
            meta.RepairNeeded = request.RepairNeeded;
            meta.Acknowledged = request.Acknowledged;
            meta.EvidenceFileName = string.IsNullOrWhiteSpace(request.EvidenceFileName) ? null : request.EvidenceFileName.Trim();
            meta.CheckedInBy = string.IsNullOrWhiteSpace(request.CheckedInBy) ? "Storekeeper" : request.CheckedInBy.Trim();
            meta.CheckinNotes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

            if (requiresMaintenance)
            {
                var maintenanceNumber = $"MNT-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
                meta.MaintenanceNumber = maintenanceNumber;

                _context.Maintenances.Add(new Maintenance
                {
                    MaintenanceNumber = maintenanceNumber,
                    Type = MaintenanceType.Corrective,
                    MaintenanceDate = DateTime.UtcNow,
                    Description = BuildMaintenanceDescription(request),
                    Cost = 0,
                    Status = "Pending",
                    AssetId = entity.Id,
                });
            }

            checkoutRequest.Status = "Returned";
            checkoutRequest.Remarks = JsonSerializer.Serialize(meta);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Fetch back with navigation properties
        var asset = await _context.Assets
            .AsNoTracking()
            .Include(a => a.Product)
            .Include(a => a.Category)
            .Include(a => a.Division)
            .Include(a => a.Supplier)
            .Include(a => a.AssignedUser)
            .Where(a => a.Id == entity.Id)
            .Select(a => new AssetDto
            {
                Id = a.Id,
                AssetCode = a.AssetCode,
                AssetTag = a.AssetTag,
                AssetDate = a.AssetDate,
                Status = a.Status,
                SerialNumber = a.SerialNumber,
                PurchaseValue = a.PurchaseValue,
                Warranty = a.Warranty,
                Notes = a.Notes,
                CategoryId = a.CategoryId,
                CategoryName = a.Category.Name,
                DivisionId = a.DivisionId,
                DivisionName = a.Division.Name,
                ProductId = a.ProductId,
                ProductName = a.Product.Name,
                SupplierId = a.SupplierId,
                SupplierName = a.Supplier.Name,
                AssignedUserId = a.AssignedUserId,
                AssignedUserName = a.AssignedUser != null ? $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}" : null
            })
            .FirstAsync(cancellationToken);

        return asset;
    }

    private static CheckinCheckoutRecordMeta ParseMeta(string? remarks)
    {
        if (string.IsNullOrWhiteSpace(remarks))
        {
            return new CheckinCheckoutRecordMeta();
        }

        try
        {
            return JsonSerializer.Deserialize<CheckinCheckoutRecordMeta>(remarks) ?? new CheckinCheckoutRecordMeta();
        }
        catch
        {
            return new CheckinCheckoutRecordMeta();
        }
    }

    private static string BuildMaintenanceDescription(CheckinAssetCommand request)
    {
        var notes = string.IsNullOrWhiteSpace(request.Notes) ? "No notes provided" : request.Notes.Trim();
        var severity = string.IsNullOrWhiteSpace(request.DamageSeverity) ? "Unspecified" : request.DamageSeverity.Trim();
        var evidence = string.IsNullOrWhiteSpace(request.EvidenceFileName) ? "No evidence file" : request.EvidenceFileName.Trim();

        return $"Auto-created on check-in. Condition: {request.Condition}; Severity: {severity}; Evidence: {evidence}; Notes: {notes}";
    }
}
