using MediatR;
using Assura.Application.Features.QueueItems.DTOs;
using Assura.Application.Common.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.QueueItems.Queries.GetAll;

public record GetAllQueueItemsQuery : IRequest<List<QueueItemDto>>;

public class GetAllQueueItemsQueryHandler : IRequestHandler<GetAllQueueItemsQuery, List<QueueItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAllQueueItemsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<QueueItemDto>> Handle(GetAllQueueItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await _context.QueueItems.AsNoTracking().OrderByDescending(q => q.Date).ToListAsync(cancellationToken);
        return _mapper.Map<List<QueueItemDto>>(items);
    }
}
