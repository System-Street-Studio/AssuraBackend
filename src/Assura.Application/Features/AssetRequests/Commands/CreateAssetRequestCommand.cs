using MediatR;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Assura.Domain.Enums;

namespace Assura.Application.Features.AssetRequests.Commands;

// Request
public record CreateAssetRequestCommand : IRequest<int>
{
    public required string EmployeeId { get; set; }   
    public required string SubmittedBy { get; set; }    
    public required string AssetCategory { get; set; }
    public required string AssetName { get; set; }
    public required string Description { get; set; }
    public required string Reason { get; set; }
    public int Quantity { get; set; }
    public required string Priority { get; set; }
    public required string RequestType { get; set; }
    public DateTime SubmittedDate { get; set; }
}
public class CreateAssetRequestHandler : IRequestHandler<CreateAssetRequestCommand, int>
{
    private readonly IApplicationDbContext _context;
    public CreateAssetRequestHandler(IApplicationDbContext context) => _context = context;

    public async Task<int> Handle(CreateAssetRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = new AssetRequest
        {
            // Mapping 
            RequesterId = request.EmployeeId,      // EmployeeId -> RequesterId
            RequesterName = request.SubmittedBy,    // SubmittedBy -> RequesterName
            SubmittedDate = request.SubmittedDate,  // Frontend Date -> Database Date
            
            AssetCategory = request.AssetCategory,
            AssetName = request.AssetName,
            Description = request.Description,
            Reason = request.Reason,

            Quantity = request.Quantity,
            Priority = request.Priority,
            RequestType = request.RequestType,
            Status = Domain.Enums.RequestStatus.Pending
        };

        _context.AssetRequests.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}