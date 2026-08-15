using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Assura.Application.Features.Assets.Queries;

/// <summary>
/// Query to retrieve all checkout and return records for the checkout history table.
/// Returns records with status "Checked Out", "Returned", or dynamically computed "Overdue".
/// </summary>
public record GetCheckoutRecordsQuery : IRequest<List<CheckoutRecordDto>>;

/// <summary>
/// DTO representing a single checkout/return record shown in the checkout history.
/// Combines data from the Request entity, the Asset, the User, and JSON metadata.
/// </summary>
public class CheckoutRecordDto
{
    public string Id { get; set; } = string.Empty;
    public string AssetId { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Serial { get; set; } = string.Empty;
    public string CheckedOutTo { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CheckoutDate { get; set; } = string.Empty;
    public string DueDate { get; set; } = string.Empty;
    public string? ReturnDate { get; set; }
    public string? Condition { get; set; }
    public string? DamageSeverity { get; set; }
    public bool RepairNeeded { get; set; }
    public bool Acknowledged { get; set; }
    public string? EvidenceFileName { get; set; }
    public string? MaintenanceNumber { get; set; }
    public string? CheckoutNotes { get; set; }
    public string? CheckinNotes { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CheckedOutBy { get; set; } = string.Empty;
    public string? CheckedInBy { get; set; }
}

/// <summary>
/// Internal model for deserializing the JSON metadata stored in Request.Remarks.
/// Tracks checkout/checkin details like due date, condition, damage info, and who performed the action.
/// </summary>
internal class CheckoutRecordMeta
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
/// Handler for <see cref="GetCheckoutRecordsQuery"/>.
/// Fetches all checkout/return Request records, parses their JSON metadata,
/// dynamically computes overdue status, and maps everything into <see cref="CheckoutRecordDto"/>.
/// </summary>
public class GetCheckoutRecordsQueryHandler : IRequestHandler<GetCheckoutRecordsQuery, List<CheckoutRecordDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCheckoutRecordsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CheckoutRecordDto>> Handle(GetCheckoutRecordsQuery request, CancellationToken cancellationToken)
    {
        var rows = await _context.Requests
            .AsNoTracking()
            .Where(r => r.AssetId != null && (r.Status == "Checked Out" || r.Status == "Returned"))
            .Include(r => r.Asset!)
                .ThenInclude(a => a.Product)
            .Include(r => r.Asset!)
                .ThenInclude(a => a.Category)
            .Include(r => r.Requester)
                .ThenInclude(u => u.Division)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return rows
            .Where(r => r.Asset != null)
            .Select(r =>
            {
                var meta = ParseMeta(r.Remarks);
                var dueDate = meta?.DueDate;
                var status = NormalizeStatus(r.Status, dueDate, today);

                return new CheckoutRecordDto
                {
                    Id = r.RequestNumber,
                    AssetId = r.AssetId!.Value.ToString(),
                    AssetName = r.Asset!.Product?.Name ?? r.Asset.AssetCode,
                    Category = r.Asset.Category?.Name ?? "-",
                    Serial = string.IsNullOrWhiteSpace(r.Asset.SerialNumber) ? "-" : r.Asset.SerialNumber,
                    CheckedOutTo = ((r.Requester?.FirstName ?? string.Empty) + " " + (r.Requester?.LastName ?? string.Empty)).Trim(),
                    Division = r.Requester?.Division?.Name ?? "N/A",
                    Email = r.Requester?.Email ?? string.Empty,
                    CheckoutDate = DateOnly.FromDateTime(r.CreatedAt).ToString("yyyy-MM-dd"),
                    DueDate = dueDate?.ToString("yyyy-MM-dd") ?? DateOnly.FromDateTime(r.CreatedAt).ToString("yyyy-MM-dd"),
                    ReturnDate = status == "Returned" ? DateOnly.FromDateTime(r.UpdatedAt ?? r.CreatedAt).ToString("yyyy-MM-dd") : null,
                    Condition = meta?.Condition,
                    DamageSeverity = meta?.DamageSeverity,
                    RepairNeeded = meta?.RepairNeeded ?? false,
                    Acknowledged = meta?.Acknowledged ?? false,
                    EvidenceFileName = meta?.EvidenceFileName,
                    MaintenanceNumber = meta?.MaintenanceNumber,
                    CheckoutNotes = r.Description,
                    CheckinNotes = meta?.CheckinNotes,
                    Status = status,
                    CheckedOutBy = string.IsNullOrWhiteSpace(meta?.CheckedOutBy) ? "Storekeeper" : meta!.CheckedOutBy!,
                    CheckedInBy = meta?.CheckedInBy
                };
            })
            .ToList();
    }

    /// <summary>
    /// Safely deserializes the JSON Remarks field into a CheckoutRecordMeta object.
    /// Returns null if the field is empty or contains invalid JSON.
    /// </summary>
    private static CheckoutRecordMeta? ParseMeta(string? remarks)
    {
        if (string.IsNullOrWhiteSpace(remarks))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<CheckoutRecordMeta>(remarks);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Dynamically determines the display status of a checkout record.
    /// If the persisted status is "Checked Out" but the due date has passed, returns "Overdue".
    /// </summary>
    private static string NormalizeStatus(string? persistedStatus, DateOnly? dueDate, DateOnly today)
    {
        if (string.Equals(persistedStatus, "Returned", StringComparison.OrdinalIgnoreCase))
        {
            return "Returned";
        }

        if (dueDate.HasValue && dueDate.Value < today)
        {
            return "Overdue";
        }

        return "Checked Out";
    }
}
