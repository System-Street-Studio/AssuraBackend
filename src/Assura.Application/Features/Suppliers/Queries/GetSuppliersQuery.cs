using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Suppliers.Queries;

public record GetSuppliersQuery : IRequest<List<SupplierDto>>;

public class GetSuppliersQueryHandler : IRequestHandler<GetSuppliersQuery, List<SupplierDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSuppliersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SupplierDto>> Handle(GetSuppliersQuery request, CancellationToken cancellationToken)
    {
        return await _context.Suppliers
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
    }
}
