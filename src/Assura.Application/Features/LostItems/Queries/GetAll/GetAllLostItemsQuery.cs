using MediatR;
using Assura.Application.Features.LostItems.DTOs;
using Assura.Application.Common.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.LostItems.Queries.GetAll;

public record GetAllLostItemsQuery : IRequest<List<LostItemDto>>;

public class GetAllLostItemsQueryHandler : IRequestHandler<GetAllLostItemsQuery, List<LostItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAllLostItemsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<LostItemDto>> Handle(GetAllLostItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await _context.LostItems.AsNoTracking().OrderByDescending(x => x.Date).ToListAsync(cancellationToken);
        return _mapper.Map<List<LostItemDto>>(items);
    }
}
