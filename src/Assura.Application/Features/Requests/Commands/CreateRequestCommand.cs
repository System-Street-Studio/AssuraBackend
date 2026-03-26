using Assura.Application.Common.Interfaces;
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
    private readonly IApplicationDbContext _context;

    public CreateRequestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateRequestCommand request, CancellationToken cancellationToken)
    {
        var requestNumber = $"REQ-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}";

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
            Status = "Pending"
        };

        _context.Requests.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        // Notify Storekeepers & Admins
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

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
