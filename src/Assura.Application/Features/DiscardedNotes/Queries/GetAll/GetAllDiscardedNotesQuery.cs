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
        var notes = await _context.DiscardedNotes
            .AsNoTracking()
            .Where(n => n.Status == Domain.Enums.DiscardNoteStatus.Completed)
            .OrderByDescending(x => x.Date)
            .ToListAsync(cancellationToken);
        var dtos = _mapper.Map<List<DiscardedNoteDto>>(notes);

        var assetIds = notes.Where(n => n.AssetId.HasValue).Select(n => n.AssetId!.Value).Distinct().ToList();
        var assigneeByAssetId = await _context.Assets
            .AsNoTracking()
            .Include(a => a.AssignedUser)
            .Where(a => assetIds.Contains(a.Id))
            .ToDictionaryAsync(
                a => a.Id,
                a => a.AssignedUser != null ? $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}" : null,
                cancellationToken);

        for (var i = 0; i < notes.Count; i++)
        {
            if (notes[i].AssetId.HasValue && assigneeByAssetId.TryGetValue(notes[i].AssetId!.Value, out var assigneeName))
            {
                dtos[i].AssigneeName = assigneeName;
            }
        }

        return dtos;
    }
}
