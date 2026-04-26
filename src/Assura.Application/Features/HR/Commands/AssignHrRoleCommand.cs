using System.Text.Json;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.HR.Commands;

public record AssignHrRoleCommand : IRequest<bool>
{
    public int UserId { get; init; }
    public string Role { get; init; } = string.Empty;
    public int? DivisionId { get; init; }
    public string? JobTitle { get; init; }
    public string? Notes { get; init; }
    public string? ActorName { get; init; }
    public string? IpAddress { get; init; }
    public string? Device { get; init; }
}

public class AssignHrRoleCommandHandler : IRequestHandler<AssignHrRoleCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public AssignHrRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(AssignHrRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null || !Enum.TryParse<UserRole>(request.Role, true, out var parsedRole))
        {
            return false;
        }

        string divisionName = "Unassigned";

        if (request.DivisionId.HasValue)
        {
            var division = await _context.Divisions
                .FirstOrDefaultAsync(d => d.Id == request.DivisionId.Value, cancellationToken);

            if (division == null)
            {
                return false;
            }

            user.DivisionId = division.Id;
            divisionName = division.Name;
        }
        else
        {
            user.DivisionId = null;
        }

        user.Role = parsedRole;
        user.JobTitle = request.JobTitle;
        user.EmploymentStatus = "Assigned";
        user.AssignedAt = DateTime.UtcNow;

        _context.AuditLogs.Add(new AuditLog
        {
            EntityName = "HR",
            EntityId = user.Id.ToString(),
            Action = "Assigned Role",
            CreatedBy = request.ActorName,
            IpAddress = request.IpAddress,
            NewValues = JsonSerializer.Serialize(new
            {
                employee = $"{user.FirstName} {user.LastName}".Trim(),
                department = divisionName,
                role = parsedRole.ToString(),
                notes = request.Notes ?? "Role assigned by HR",
                result = "Success",
                device = request.Device
            })
        });

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
