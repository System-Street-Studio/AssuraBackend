using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.RepairingFirms.Queries;

public record GetRepairingFirmsQuery : IRequest<List<RepairingFirmDto>>;

public class GetRepairingFirmsQueryHandler : IRequestHandler<GetRepairingFirmsQuery, List<RepairingFirmDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRepairingFirmsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<RepairingFirmDto>> Handle(GetRepairingFirmsQuery request, CancellationToken cancellationToken)
    {
        return await _context.RepairingFirms
            .AsNoTracking()
            .Select(rf => new RepairingFirmDto
            {
                Id = rf.Id,
                Name = rf.Name,
                ContactPerson = rf.ContactPerson,
                Email = rf.Email,
                Phone = rf.Phone,
                Address = rf.Address
            })
            .ToListAsync(cancellationToken);
    }
}
