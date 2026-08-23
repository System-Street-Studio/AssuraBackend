using MediatR;
using Assura.Application.Features.AccPendingItems.DTOs;
using Assura.Application.Common.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.AccPendingItems.Queries.GetAll;

public record GetAllAccPendingItemsQuery : IRequest<List<AccPendingItemDto>>;

public class GetAllAccPendingItemsQueryHandler : IRequestHandler<GetAllAccPendingItemsQuery, List<AccPendingItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAllAccPendingItemsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<AccPendingItemDto>> Handle(GetAllAccPendingItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await _context.AccPendingItems.AsNoTracking().OrderByDescending(x => x.Date).ToListAsync(cancellationToken);
        var dtos = _mapper.Map<List<AccPendingItemDto>>(items);

        var assetIds = items.Where(i => i.AssetId.HasValue).Select(i => i.AssetId!.Value).Distinct().ToList();
        var assigneeByAssetId = await _context.Assets
            .AsNoTracking()
            .Include(a => a.AssignedUser)
            .Where(a => assetIds.Contains(a.Id))
            .ToDictionaryAsync(
                a => a.Id,
                a => a.AssignedUser != null ? $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}" : null,
                cancellationToken);

        var buyerIds = items.Where(i => i.BuyerId.HasValue).Select(i => i.BuyerId!.Value).Distinct().ToList();
        var buyerNameById = await _context.Buyers
            .AsNoTracking()
            .Where(b => buyerIds.Contains(b.Id))
            .ToDictionaryAsync(b => b.Id, b => b.Name, cancellationToken);

        for (var i = 0; i < items.Count; i++)
        {
            if (items[i].AssetId.HasValue && assigneeByAssetId.TryGetValue(items[i].AssetId!.Value, out var assigneeName))
            {
                dtos[i].AssigneeName = assigneeName;
            }

            if (items[i].BuyerId.HasValue && buyerNameById.TryGetValue(items[i].BuyerId!.Value, out var buyerName))
            {
                dtos[i].BuyerId = items[i].BuyerId;
                dtos[i].BuyerName = buyerName;
            }
        }

        return dtos;
    }
}
