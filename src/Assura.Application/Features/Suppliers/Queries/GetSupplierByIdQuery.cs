using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Assura.Application.Features.Suppliers.Queries;

public record GetSupplierByIdQuery(int Id) : IRequest<SupplierDto?>;

public class GetSupplierByIdQueryHandler : IRequestHandler<GetSupplierByIdQuery, SupplierDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetSupplierByIdQueryHandler> _logger;

    public GetSupplierByIdQueryHandler(IApplicationDbContext context, ILogger<GetSupplierByIdQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
        _logger.LogInformation("[DEBUG] GetSupplierByIdQueryHandler: Initialized");
    }

    public async Task<SupplierDto?> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[DEBUG] GetSupplierByIdQueryHandler: Handling query for id {Id}", request.Id);
        var supplier = await _context.Suppliers
            .AsNoTracking()
            .Where(s => s.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);

        if (supplier == null)
        {
            _logger.LogWarning("[DEBUG] GetSupplierByIdQueryHandler: Supplier with id {Id} NOT found in DB", request.Id);
        }
        else
        {
            _logger.LogInformation("[DEBUG] GetSupplierByIdQueryHandler: Supplier with id {Id} found: {Name}", request.Id, supplier.Name);
        }

        return supplier;
    }
}
