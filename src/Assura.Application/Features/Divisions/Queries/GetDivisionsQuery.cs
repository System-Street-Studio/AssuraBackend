using AutoMapper;
using AutoMapper.QueryableExtensions;
using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Divisions.Queries;

public record GetDivisionsQuery : IRequest<List<DivisionDto>>;

public class GetDivisionsQueryHandler : IRequestHandler<GetDivisionsQuery, List<DivisionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDivisionsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<DivisionDto>> Handle(GetDivisionsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Divisions
            .AsNoTracking()
            .ProjectTo<DivisionDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
