using Assura.Application.Common.Interfaces;
using Assura.Domain.Constants;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Requests.Commands;

public record CreateRequestCommand : IRequest<int>
{
    public RequestType Type { get; init; }
    public PriorityType Priority { get; init; }
    public string? Description { get; init; }
    public string? Specifications { get; init; }
    public string? SpecialNote { get; init; }
    public int RequesterId { get; init; }
    public int? AssetId { get; init; }
}

public class CreateRequestCommandHandler : IRequestHandler<CreateRequestCommand, int>
{
    private const decimal LowValueThreshold = 100000m;

    private readonly IApplicationDbContext _context;

    public CreateRequestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateRequestCommand request, CancellationToken cancellationToken)
    {
        var requestNumber = $"REQ-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        var requester = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.RequesterId, cancellationToken);

        var bypassDivisionHeadApproval = await ShouldBypassDivisionHeadApproval(request, requester, cancellationToken);
        var initialStatus = bypassDivisionHeadApproval
            ? RequestWorkflowStatus.PendingStorekeeperReview
            : RequestWorkflowStatus.PendingDivisionHeadApproval;

        var entity = new Request
        {
            RequestNumber = requestNumber,
            Type = request.Type,
            Priority = request.Priority,
            Description = request.Description,
            Specifications = request.Specifications,
            SpecialNote = request.SpecialNote,
            RequesterId = request.RequesterId,
            AssetId = request.AssetId,
            Status = initialStatus,
            RequiresDivisionHeadApproval = !bypassDivisionHeadApproval
        };

        _context.Requests.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        if (initialStatus == RequestWorkflowStatus.PendingDivisionHeadApproval && requester?.DivisionId is int divisionId)
        {
            var divisionHeads = await _context.Users
                .Where(u => u.DivisionId == divisionId && u.Role == UserRole.DivisionHead)
                .ToListAsync(cancellationToken);

            foreach (var user in divisionHeads)
            {
                _context.Notifications.Add(new Notification
                {
                    Title = "Division Approval Required",
                    Message = $"Request {requestNumber} is waiting for division head approval.",
                    UserId = user.Id,
                    Type = "Info",
                    ReferenceId = entity.Id.ToString()
                });
            }
        }
        else
        {
            var storekeepers = await _context.Users
                .Where(u => u.Role == UserRole.Storekeeper || u.Role == UserRole.Admin)
                .ToListAsync(cancellationToken);

            foreach (var user in storekeepers)
            {
                _context.Notifications.Add(new Notification
                {
                    Title = "New Asset Request",
                    Message = $"A new {request.Type} request ({requestNumber}) requires stock verification.",
                    UserId = user.Id,
                    Type = "Info",
                    ReferenceId = entity.Id.ToString()
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    private Task<bool> ShouldBypassDivisionHeadApproval(
        CreateRequestCommand request,
        User? requester,
        CancellationToken cancellationToken)
    {
        // Division Heads and Admins always bypass their own division level review
        if (requester?.Role == UserRole.DivisionHead || requester?.Role == UserRole.Admin)
        {
            return Task.FromResult(true);
        }

        // Strict requirement: All employee requests should be reviewed 
        // by their respective Division Head to ensure departmental oversight.
        return Task.FromResult(false);
    }
}
