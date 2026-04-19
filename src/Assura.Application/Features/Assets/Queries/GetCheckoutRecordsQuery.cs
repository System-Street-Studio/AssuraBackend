using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Assura.Application.Features.Assets.Queries;

public record GetCheckoutRecordsQuery : IRequest<List<CheckoutRecordDto>>;

public class CheckoutRecordDto
{
    public string Id { get; set; } = string.Empty;
    public string AssetId { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Serial { get; set; } = string.Empty;
    public string CheckedOutTo { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
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
            .Where(r => r.Type == RequestType.Asset && r.AssetId != null)
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
                    Department = r.Requester?.Division?.Name ?? "N/A",
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
