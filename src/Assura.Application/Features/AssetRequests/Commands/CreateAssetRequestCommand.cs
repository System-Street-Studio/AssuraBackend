using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.AssetRequests.Commands;

// Request
public record CreateAssetRequestCommand : IRequest<int>
{
    public string? EmployeeId { get; set; }   
    public string? SubmittedBy { get; set; }    
    public string? AssetCategory { get; set; }
    public string? AssetName { get; set; }
    public string? Description { get; set; }
    public string? Reason { get; set; }
    public int Quantity { get; set; }
    public string? Priority { get; set; }
    public string? RequestType { get; set; }
    public DateTime SubmittedDate { get; set; }
}

public class CreateAssetRequestHandler : IRequestHandler<CreateAssetRequestCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateAssetRequestHandler(IApplicationDbContext context) => _context = context;

    public async Task<int> Handle(CreateAssetRequestCommand request, CancellationToken cancellationToken)
    {
        int? userId = int.TryParse(request.EmployeeId, out var id) ? id : null;
        int? divisionId = null;

        if (userId.HasValue)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);
            divisionId = user?.DivisionId;
        }

        var entity = new AssetRequest
        {
            // Mapping 
            RequesterId = request.EmployeeId ?? "",      // EmployeeId -> RequesterId
            RequesterName = request.SubmittedBy ?? "",    // SubmittedBy -> RequesterName
            SubmittedDate = request.SubmittedDate,  // Frontend Date -> Database Date
            
            AssetCategory = request.AssetCategory ?? "",
            AssetName = request.AssetName ?? "",
            Description = request.Description ?? "",
            Reason = request.Reason ?? "",

            Quantity = request.Quantity,
            Priority = request.Priority ?? "Normal",
            RequestType = request.RequestType ?? "New Asset",
            Status = Domain.Enums.RequestStatus.Pending,
            
            UserId = userId,
            DivisionId = divisionId
        };

        _context.AssetRequests.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
