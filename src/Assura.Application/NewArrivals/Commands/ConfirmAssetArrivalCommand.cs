using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assura.Application.NewArrivals.Commands;

public record ConfirmAssetArrivalCommand(int InformingId, int UserId, string? ConfirmationRemarks = null) : IRequest<bool>;

public class ConfirmAssetArrivalCommandHandler : IRequestHandler<ConfirmAssetArrivalCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ConfirmAssetArrivalCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ConfirmAssetArrivalCommand request, CancellationToken cancellationToken)
    {
        var informing = await _context.AssetInformings
            .Include(a => a.Division)
            .FirstOrDefaultAsync(a => a.Id == request.InformingId, cancellationToken);

        if (informing == null)
            throw new Exception("Asset arrival record not found.");

        informing.Status = "Confirmed";
        informing.UpdatedAt = DateTime.UtcNow;

        var employee = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
        var employeeName = employee != null ? $"{employee.FirstName} {employee.LastName}".Trim() : "Employee";

        // Notify Storekeepers
        var storekeepers = await _context.Users
            .Where(u => u.Role == UserRole.Storekeeper && u.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var sk in storekeepers)
        {
            _context.Notifications.Add(new Notification
            {
                Title = "Arrival Confirmed by Employee",
                Message = $"{employeeName} has confirmed the arrival of '{informing.ItemName}'. {(string.IsNullOrEmpty(request.ConfirmationRemarks) ? "" : $"Note: {request.ConfirmationRemarks}")}",
                UserId = sk.Id,
                Type = "Success",
                ReferenceId = informing.Id.ToString()
            });
        }

        // Also notify division heads if applicable
        var divisionHeads = await _context.Users
            .Where(u => u.Role == UserRole.DivisionHead && u.DivisionId == informing.DivisionId && u.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var dh in divisionHeads)
        {
            _context.Notifications.Add(new Notification
            {
                Title = "Division Asset Confirmed",
                Message = $"{employeeName} has confirmed receipt of '{informing.ItemName}' for {informing.Division?.Name ?? "Division"}.",
                UserId = dh.Id,
                Type = "Info",
                ReferenceId = informing.Id.ToString()
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
