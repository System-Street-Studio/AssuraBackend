using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Requests.Queries;

public record GetRequestByIdQuery(int Id) : IRequest<RequestDto?>;

public class GetRequestByIdQueryHandler : IRequestHandler<GetRequestByIdQuery, RequestDto?>
{
    private readonly IApplicationDbContext _context;

    public GetRequestByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RequestDto?> Handle(GetRequestByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Requests
            .AsNoTracking()
            .Include(r => r.Requester)
            .Include(r => r.Requester.Division)
            .Include(r => r.Asset)
            .Where(r => r.Id == request.Id)
            .Select(r => new RequestDto
            {
                Id = r.Id,
                RequesterId = r.RequesterId,
                RequestNumber = r.RequestNumber,
                Type = r.Type.ToString(),
                Priority = r.Priority.ToString(),
                Description = r.Description,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                RequesterName = $"{r.Requester.FirstName} {r.Requester.LastName}",
                Department = r.Requester.Division != null ? r.Requester.Division.Name : "N/A",
                AssetName = r.Asset != null ? r.Asset.AssetCode : null
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
