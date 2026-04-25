using MediatR;
using Assura.Application.Features.Assets.DTOs;
using Assura.Application.Common.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Assets.Queries.GetAll;

public record GetAllAssetsQuery : IRequest<List<AssetDto>>;

public class GetAllAssetsQueryHandler : IRequestHandler<GetAllAssetsQuery, List<AssetDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAllAssetsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<AssetDto>> Handle(GetAllAssetsQuery request, CancellationToken cancellationToken)
    {
        var assets = await _context.Assets
            .Include(a => a.Product)
            .Include(a => a.Category)
            .Include(a => a.Division)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<AssetDto>>(assets);
    }
}
