using MediatR;
using Assura.Application.Features.DiscardedNotes.DTOs;
using Assura.Application.Common.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.DiscardedNotes.Queries.GetAll;

public record GetAllDiscardedNotesQuery : IRequest<List<DiscardedNoteDto>>;

public class GetAllDiscardedNotesQueryHandler : IRequestHandler<GetAllDiscardedNotesQuery, List<DiscardedNoteDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAllDiscardedNotesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<DiscardedNoteDto>> Handle(GetAllDiscardedNotesQuery request, CancellationToken cancellationToken)
    {
        var notes = await _context.DiscardedNotes.AsNoTracking().ToListAsync(cancellationToken);
        return _mapper.Map<List<DiscardedNoteDto>>(notes);
    }
}
