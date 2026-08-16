using System.Text.Json;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.HR.Commands;

/// <summary>
/// Reverses a previous <see cref="RejectHrUserCommand"/> — puts a rejected user back into
/// the pending-assignment pool instead of leaving rejection as a permanent, unrecoverable
/// dead end for HR.
/// </summary>
public record ReconsiderHrUserCommand : IRequest<bool>
{
    public int UserId { get; init; }
    public string? Notes { get; init; }
    public string? ActorName { get; init; }
    public string? IpAddress { get; init; }
    public string? Device { get; init; }
}

public class ReconsiderHrUserCommandValidator : AbstractValidator<ReconsiderHrUserCommand>
{
    public ReconsiderHrUserCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public class ReconsiderHrUserCommandHandler : IRequestHandler<ReconsiderHrUserCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ReconsiderHrUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ReconsiderHrUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Division)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null || user.EmploymentStatus != "Rejected")
        {
            return false;
        }

        user.IsActive = true;
        user.EmploymentStatus = "PendingAssignment";

        _context.Notifications.Add(new Notification
        {
            Title = "Registration Reconsidered",
            Message = "HR has reconsidered your registration. You may log in again — your account is now pending role assignment.",
            UserId = user.Id,
            Type = "Info"
        });

        _context.AuditLogs.Add(new AuditLog
        {
            EntityName = "HR",
            EntityId = user.Id.ToString(),
            Action = "Reconsidered Rejection",
            CreatedBy = request.ActorName,
            IpAddress = request.IpAddress,
            NewValues = JsonSerializer.Serialize(new
            {
                employee = $"{user.FirstName} {user.LastName}".Trim(),
                department = user.Division?.Name ?? "Unassigned",
                notes = request.Notes,
                result = "Reconsidered",
                device = request.Device
            })
        });

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
