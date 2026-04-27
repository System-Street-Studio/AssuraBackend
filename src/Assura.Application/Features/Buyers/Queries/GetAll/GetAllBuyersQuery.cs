using MediatR;
using Assura.Application.Features.Buyers.DTOs;
using Assura.Application.Common.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Buyers.Queries.GetAll;

public record GetAllBuyersQuery : IRequest<List<BuyerDto>>;

public class GetAllBuyersQueryHandler : IRequestHandler<GetAllBuyersQuery, List<BuyerDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAllBuyersQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<BuyerDto>> Handle(GetAllBuyersQuery request, CancellationToken cancellationToken)
    {
        var buyers = await _context.Buyers.AsNoTracking().ToListAsync(cancellationToken);
        return _mapper.Map<List<BuyerDto>>(buyers);
    }
}
