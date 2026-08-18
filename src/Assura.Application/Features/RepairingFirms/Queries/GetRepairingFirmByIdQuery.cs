using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.RepairingFirms.Queries;

public record GetRepairingFirmByIdQuery(int Id) : IRequest<RepairingFirmDto?>;

public class GetRepairingFirmByIdQueryHandler : IRequestHandler<GetRepairingFirmByIdQuery, RepairingFirmDto?>
{
    private readonly IApplicationDbContext _context;

    public GetRepairingFirmByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RepairingFirmDto?> Handle(GetRepairingFirmByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.RepairingFirms
            .AsNoTracking()
            .Where(rf => rf.Id == request.Id)
            .Select(rf => new RepairingFirmDto
            {
                Id = rf.Id,
                Name = rf.Name,
                ContactPerson = rf.ContactPerson,
                Email = rf.Email,
                Phone = rf.Phone,
                Address = rf.Address
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
