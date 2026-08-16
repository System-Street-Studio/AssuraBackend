using System.Text.Json;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.HR.Commands;

public record RejectHrUserCommand : IRequest<bool>
{
    public int UserId { get; init; }
    public string? Notes { get; init; }
    public string? ActorName { get; init; }
    public string? IpAddress { get; init; }
    public string? Device { get; init; }
}

public class RejectHrUserCommandValidator : AbstractValidator<RejectHrUserCommand>
{
    public RejectHrUserCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.Notes).NotEmpty().MaximumLength(500);
    }
}

public class RejectHrUserCommandHandler : IRequestHandler<RejectHrUserCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public RejectHrUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(RejectHrUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Division)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return false;
        }

        user.Role = null;
        user.EmploymentStatus = "Rejected";
        user.IsActive = false;

        _context.Notifications.Add(new Notification
        {
            Title = "Registration Rejected",
            Message = $"Your registration was rejected by HR.{(string.IsNullOrWhiteSpace(request.Notes) ? "" : $" Reason: {request.Notes}")} Contact HR for more information.",
            UserId = user.Id,
            Type = "Error"
        });

        _context.AuditLogs.Add(new AuditLog
        {
            EntityName = "HR",
            EntityId = user.Id.ToString(),
            Action = "Rejected Registration",
            CreatedBy = request.ActorName,
            IpAddress = request.IpAddress,
            NewValues = JsonSerializer.Serialize(new
            {
                employee = $"{user.FirstName} {user.LastName}".Trim(),
                department = user.Division?.Name ?? "Unassigned",
                role = user.RequestedRole ?? "Unassigned",
                notes = request.Notes,
                result = "Rejected",
                device = request.Device
            })
        });

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
