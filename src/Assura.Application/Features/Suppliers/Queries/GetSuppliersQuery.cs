using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Assura.Application.Features.Suppliers.Queries;

public record GetSuppliersQuery : IRequest<List<SupplierDto>>;

public class GetSuppliersQueryHandler : IRequestHandler<GetSuppliersQuery, List<SupplierDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetSuppliersQueryHandler> _logger;

    public GetSuppliersQueryHandler(IApplicationDbContext context, ILogger<GetSuppliersQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<SupplierDto>> Handle(GetSuppliersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[DEBUG] GetSuppliersQueryHandler: Fetching suppliers from DB");
        var suppliers = await _context.Suppliers
            .AsNoTracking()
            .Select(s => new SupplierDto
            {
                Id = s.Id,
                Name = s.Name,
                ContactPerson = s.ContactPerson,
                Email = s.Email,
                Phone = s.Phone,
                Address = s.Address,
                Website = s.Website,
                Status = "Active",
                CreatedAt = s.CreatedAt
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("[DEBUG] GetSuppliersQueryHandler: Found {Count} suppliers", suppliers.Count);
        return suppliers;
    }
}
