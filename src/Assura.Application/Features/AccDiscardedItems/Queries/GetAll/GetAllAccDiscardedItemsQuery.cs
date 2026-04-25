using MediatR;
using Assura.Application.Features.AccDiscardedItems.DTOs;
using Assura.Application.Common.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.AccDiscardedItems.Queries.GetAll;

public record GetAllAccDiscardedItemsQuery : IRequest<List<AccDiscardedItemDto>>;

public class GetAllAccDiscardedItemsQueryHandler : IRequestHandler<GetAllAccDiscardedItemsQuery, List<AccDiscardedItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAllAccDiscardedItemsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<AccDiscardedItemDto>> Handle(GetAllAccDiscardedItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await _context.AccDiscardedItems.AsNoTracking().OrderByDescending(x => x.Date).ToListAsync(cancellationToken);
        return _mapper.Map<List<AccDiscardedItemDto>>(items);
    }
}
