using MediatR;
using Assura.Application.Features.AccDiscardNotes.DTOs;
using Assura.Application.Common.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.AccDiscardNotes.Queries.GetAll;

public record GetAllAccDiscardNotesQuery : IRequest<List<AccDiscardNoteDto>>;

public class GetAllAccDiscardNotesQueryHandler : IRequestHandler<GetAllAccDiscardNotesQuery, List<AccDiscardNoteDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAllAccDiscardNotesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<AccDiscardNoteDto>> Handle(GetAllAccDiscardNotesQuery request, CancellationToken cancellationToken)
    {
        var items = await _context.AccDiscardNotes.AsNoTracking().OrderByDescending(x => x.Date).ToListAsync(cancellationToken);
        return _mapper.Map<List<AccDiscardNoteDto>>(items);
    }
}
