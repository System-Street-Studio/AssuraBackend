using System.Text.Json;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.HR.Commands;

public record DivisionRoleAssignment
{
    public int DivisionId { get; init; }
    public string Role { get; init; } = string.Empty;
}

public record AssignHrRoleCommand : IRequest<bool>
{
    public int UserId { get; init; }
    public List<DivisionRoleAssignment> Assignments { get; init; } = new();
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
            .Include(u => u.DivisionRoles)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null || request.Assignments.Count == 0)
        {
            return false;
        }

        // Clear existing assignments if any
        user.DivisionRoles.Clear();

        foreach (var assignment in request.Assignments)
        {
            if (!Enum.TryParse<UserRole>(assignment.Role, true, out var parsedRole))
            {
                continue;
            }

            var division = await _context.Divisions
                .FirstOrDefaultAsync(d => d.Id == assignment.DivisionId, cancellationToken);

            if (division == null) continue;

            user.DivisionRoles.Add(new UserDivisionRole
            {
                UserId = user.Id,
                DivisionId = division.Id,
                Role = parsedRole,
                JobTitle = request.JobTitle,
                Notes = request.Notes,
                AssignedAt = DateTime.UtcNow
            });
        }

        if (user.DivisionRoles.Count == 0) return false;

        // Set primary role/division for compatibility
        var primary = user.DivisionRoles.First();
        user.Role = primary.Role;
        user.DivisionId = primary.DivisionId;
        user.JobTitle = request.JobTitle;
        user.EmploymentStatus = "Assigned";
        user.AssignedAt = DateTime.UtcNow;

        _context.AuditLogs.Add(new AuditLog
        {
            EntityName = "HR",
            EntityId = user.Id.ToString(),
            Action = "Assigned Roles",
            CreatedBy = request.ActorName,
            IpAddress = request.IpAddress,
            NewValues = JsonSerializer.Serialize(new
            {
                employee = $"{user.FirstName} {user.LastName}".Trim(),
                assignments = request.Assignments.Select(a => new { a.DivisionId, a.Role }),
                notes = request.Notes ?? "Role(s) assigned by HR",
                result = "Success",
                device = request.Device
            })
        });

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
