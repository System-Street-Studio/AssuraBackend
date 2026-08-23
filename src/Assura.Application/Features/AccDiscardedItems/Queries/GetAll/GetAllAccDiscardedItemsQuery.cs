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
        var dtos = _mapper.Map<List<AccDiscardedItemDto>>(items);

        var buyerIds = items.Where(i => i.BuyerId.HasValue).Select(i => i.BuyerId!.Value).Distinct().ToList();
        var buyerNameById = await _context.Buyers
            .AsNoTracking()
            .Where(b => buyerIds.Contains(b.Id))
            .ToDictionaryAsync(b => b.Id, b => b.Name, cancellationToken);

        for (var i = 0; i < items.Count; i++)
        {
            if (items[i].BuyerId.HasValue && buyerNameById.TryGetValue(items[i].BuyerId!.Value, out var buyerName))
            {
                dtos[i].BuyerId = items[i].BuyerId;
                dtos[i].BuyerName = buyerName;
            }
        }

        return dtos;
    }
}
