using System.Text.Json;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.HR.Commands;

public record UpdateHrUserCommand : IRequest<bool>
{
    public int UserId { get; init; }
    public int? DivisionId { get; init; }
    public string? Role { get; init; }
    public string? JobTitle { get; init; }
    public string? PhoneNumber { get; init; }
    public string? RequestedRole { get; init; }
    public string? EmploymentStatus { get; init; }
    public string? Notes { get; init; }
    public string? ActorName { get; init; }
    public string? IpAddress { get; init; }
    public string? Device { get; init; }
}

public class UpdateHrUserCommandValidator : AbstractValidator<UpdateHrUserCommand>
{
    public UpdateHrUserCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.PhoneNumber).MaximumLength(30);
        RuleFor(x => x.JobTitle).MaximumLength(100);
        RuleFor(x => x.RequestedRole).MaximumLength(50);
        RuleFor(x => x.EmploymentStatus).MaximumLength(40);
    }
}

public class UpdateHrUserCommandHandler : IRequestHandler<UpdateHrUserCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateHrUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateHrUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Division)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return false;
        }

        string divisionName = user.Division?.Name ?? "Unassigned";

        if (request.DivisionId.HasValue)
        {
            var division = await _context.Divisions.FirstOrDefaultAsync(d => d.Id == request.DivisionId.Value, cancellationToken);
            if (division == null)
            {
                return false;
            }

            user.DivisionId = division.Id;
            divisionName = division.Name;
        }

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            if (!Enum.TryParse<UserRole>(request.Role, true, out var parsedRole))
            {
                return false;
            }

            user.Role = parsedRole;
        }

        if (request.JobTitle is not null)
        {
            user.JobTitle = request.JobTitle;
        }

        if (request.PhoneNumber is not null)
        {
            user.PhoneNumber = request.PhoneNumber;
        }

        if (request.RequestedRole is not null)
        {
            user.RequestedRole = request.RequestedRole;
        }

        if (!string.IsNullOrWhiteSpace(request.EmploymentStatus))
        {
            user.EmploymentStatus = request.EmploymentStatus;
        }

        _context.AuditLogs.Add(new AuditLog
        {
            EntityName = "HR",
            EntityId = user.Id.ToString(),
            Action = "Updated Employee Details",
            CreatedBy = request.ActorName,
            IpAddress = request.IpAddress,
            NewValues = JsonSerializer.Serialize(new
            {
                employee = $"{user.FirstName} {user.LastName}".Trim(),
                department = divisionName,
                role = user.Role?.ToString() ?? "Unassigned",
                notes = request.Notes ?? "Employee details updated by HR",
                result = "Success",
                device = request.Device
            })
        });

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
