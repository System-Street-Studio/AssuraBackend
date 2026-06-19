using MediatR;
using Assura.Application.Features.Receipts.DTOs;
using Assura.Application.Common.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Receipts.Queries.GetAll;

public record GetAllReceiptsQuery : IRequest<List<ReceiptDto>>;

public class GetAllReceiptsQueryHandler : IRequestHandler<GetAllReceiptsQuery, List<ReceiptDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAllReceiptsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ReceiptDto>> Handle(GetAllReceiptsQuery request, CancellationToken cancellationToken)
    {
        var receipts = await _context.Receipts.AsNoTracking().OrderByDescending(r => r.Date).ToListAsync(cancellationToken);
        return _mapper.Map<List<ReceiptDto>>(receipts);
    }
}
