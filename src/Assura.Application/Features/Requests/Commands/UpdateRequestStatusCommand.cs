using Assura.Application.Common.Interfaces;
using Assura.Domain.Constants;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Requests.Commands;

public record UpdateRequestStatusCommand : IRequest
{
    public int Id { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Notes { get; init; }

    // Set by the controller from the caller's JWT role claim — defense-in-depth, matching
    // ProcessRequestCommand.CallerRole; see that command's comment for why.
    public string? CallerRole { get; init; }
}

// This endpoint is only ever called with "Approved" or "Rejected" by the storekeeper
// approve/reject flow (see inventory RequestService.approve/reject). Restricting it here
// stops an arbitrary status string from silently corrupting entity.Status and breaking
// every other handler's string comparisons against it.
public class UpdateRequestStatusCommandValidator : AbstractValidator<UpdateRequestStatusCommand>
{
    public UpdateRequestStatusCommandValidator()
    {
        RuleFor(x => x.Status)
            .Must(s => s == RequestWorkflowStatus.Approved || s == RequestWorkflowStatus.Rejected)
            .WithMessage($"Status must be '{RequestWorkflowStatus.Approved}' or '{RequestWorkflowStatus.Rejected}'.");
    }
}

public class UpdateRequestStatusCommandHandler : IRequestHandler<UpdateRequestStatusCommand>
{
    private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        Roles.Storekeeper, Roles.Admin, Roles.Procurement
    };

    private readonly IApplicationDbContext _context;

    public UpdateRequestStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateRequestStatusCommand request, CancellationToken cancellationToken)
    {
        if (request.CallerRole == null || !AllowedRoles.Contains(request.CallerRole))
        {
            throw new UnauthorizedAccessException("Only Storekeeper, Procurement, or Admin may update a request's status.");
        }

        // Negative ID means this is an AssetRequest record (from the unified /requests list) —
        // matches the fallback ProcessRequestCommand/ConfirmTemporaryAssignmentCommand already use.
        if (request.Id < 0)
        {
            var actualId = Math.Abs(request.Id);
            var assetRequest = await _context.AssetRequests
                .FirstOrDefaultAsync(ar => ar.Id == actualId, cancellationToken);

            if (assetRequest == null) return;

            assetRequest.Status = request.Status == RequestWorkflowStatus.Approved
                ? RequestStatus.Approved
                : RequestStatus.Rejected;

            if (!string.IsNullOrEmpty(request.Notes))
            {
                assetRequest.Reason = (assetRequest.Reason ?? "") + " (Remarks: " + request.Notes + ")";
            }

            int? requesterIdVal = assetRequest.UserId;
            if (!requesterIdVal.HasValue && int.TryParse(assetRequest.RequesterId, out var rid))
            {
                requesterIdVal = rid;
            }

            _context.Notifications.Add(new Notification
            {
                Title = $"Request {request.Status}",
                Message = $"Your request for '{assetRequest.AssetName}' has been {request.Status.ToLower()}.",
                UserId = requesterIdVal ?? 0,
                Type = request.Status == RequestWorkflowStatus.Approved ? "Success" : "Error",
                ReferenceId = assetRequest.Id.ToString()
            });

            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        var entity = await _context.Requests
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (entity == null) return;

        entity.Status = request.Status;
        if (!string.IsNullOrEmpty(request.Notes))
        {
            entity.Remarks = request.Notes;
        }

        // Add a notification for the requester
        _context.Notifications.Add(new Notification
        {
            Title = $"Request {request.Status}",
            Message = $"Your request {entity.RequestNumber} has been {request.Status.ToLower()}.",
            UserId = entity.RequesterId,
            Type = request.Status == RequestWorkflowStatus.Approved ? "Success" : "Error",
            ReferenceId = entity.Id.ToString()
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
