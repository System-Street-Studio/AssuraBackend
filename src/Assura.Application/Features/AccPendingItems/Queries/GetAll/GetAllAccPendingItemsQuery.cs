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
        return _mapper.Map<List<AccPendingItemDto>>(items);
    }
}
