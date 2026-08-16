using System.Text.Json;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Constants;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.HR.Commands;

public record DivisionRoleAssignment
{
    public int DivisionId { get; init; }
    public string Role { get; init; } = string.Empty;
}

public record AssignHrRoleCommand : IRequest<AssignHrRoleResult>
{
    public int UserId { get; init; }
    public List<DivisionRoleAssignment> Assignments { get; init; } = new();
    public string? JobTitle { get; init; }
    public string? Notes { get; init; }
    public string? ActorName { get; init; }
    public string? IpAddress { get; init; }
    public string? Device { get; init; }
}

public record AssignHrRoleResult
{
    public bool Success { get; init; }

    /// <summary>
    /// Assignments from the request that were dropped instead of applied — either because
    /// the role wasn't a valid/HR-assignable <see cref="UserRole"/>, or the division didn't
    /// exist. Non-empty even on a successful (<see cref="Success"/> = true) result means the
    /// request partially succeeded and the caller should be told which entries were skipped.
    /// </summary>
    public List<DivisionRoleAssignment> SkippedAssignments { get; init; } = new();
}

public class AssignHrRoleCommandValidator : AbstractValidator<AssignHrRoleCommand>
{
    public AssignHrRoleCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.Assignments).NotEmpty();
        RuleForEach(x => x.Assignments).ChildRules(assignment =>
        {
            assignment.RuleFor(a => a.DivisionId).GreaterThan(0);
            assignment.RuleFor(a => a.Role).NotEmpty().MaximumLength(50);
        });
        RuleFor(x => x.JobTitle).MaximumLength(100);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public class AssignHrRoleCommandHandler : IRequestHandler<AssignHrRoleCommand, AssignHrRoleResult>
{
    private readonly IApplicationDbContext _context;

    public AssignHrRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AssignHrRoleResult> Handle(AssignHrRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.DivisionRoles)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null || request.Assignments.Count == 0)
        {
            return new AssignHrRoleResult { Success = false };
        }

        // Clear existing assignments if any
        user.DivisionRoles.Clear();

        var skipped = new List<DivisionRoleAssignment>();

        foreach (var assignment in request.Assignments)
        {
            if (!Enum.TryParse<UserRole>(assignment.Role, true, out var parsedRole) ||
                !Roles.HrAssignableRoles.Contains(parsedRole))
            {
                skipped.Add(assignment);
                continue;
            }

            var division = await _context.Divisions
                .FirstOrDefaultAsync(d => d.Id == assignment.DivisionId, cancellationToken);

            if (division == null)
            {
                skipped.Add(assignment);
                continue;
            }

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

        if (user.DivisionRoles.Count == 0)
        {
            return new AssignHrRoleResult { Success = false, SkippedAssignments = skipped };
        }

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
                skipped = skipped.Select(a => new { a.DivisionId, a.Role }),
                notes = request.Notes ?? "Role(s) assigned by HR",
                result = skipped.Count > 0 ? "Partial Success" : "Success",
                device = request.Device
            })
        });

        await _context.SaveChangesAsync(cancellationToken);
        return new AssignHrRoleResult { Success = true, SkippedAssignments = skipped };
    }
}
