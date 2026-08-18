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
        var discardedNotes = await _context.DiscardedNotes
            .AsNoTracking()
            .Where(n => n.Status == Domain.Enums.DiscardNoteStatus.Completed)
            .OrderByDescending(x => x.Date)
            .ToListAsync(cancellationToken);
        
        if (discardedNotes.Count > 0)
        {
            var assetIds = discardedNotes.Where(n => n.AssetId.HasValue).Select(n => n.AssetId!.Value).Distinct().ToList();
            var assetsById = await _context.Assets
                .AsNoTracking()
                .Include(a => a.AssignedUser)
                .Where(a => assetIds.Contains(a.Id))
                .ToDictionaryAsync(a => a.Id, cancellationToken);

            var result = new List<AccDiscardNoteDto>();

            foreach (var note in discardedNotes)
            {
                var asset = note.AssetId.HasValue && assetsById.TryGetValue(note.AssetId.Value, out var a) ? a : null;
                var userName = asset?.AssignedUser != null 
                    ? $"{asset.AssignedUser.FirstName} {asset.AssignedUser.LastName}" 
                    : (note.RequestedByName ?? "Unassigned");

                result.Add(new AccDiscardNoteDto
                {
                    Id = note.Id.ToString(),
                    AssetName = note.Name,
                    Division = note.Division,
                    Date = note.Date.ToString("dd MMM yyyy"),
                    Note = !string.IsNullOrWhiteSpace(note.SpecialNote) ? note.SpecialNote : "N/A",
                    Status = note.Status == Domain.Enums.DiscardNoteStatus.Completed ? "Approved" : note.Status.ToString(),
                    AssetType = !string.IsNullOrWhiteSpace(note.AssetType) ? note.AssetType : "General",
                    CurrentUser = userName,
                    Time = note.Time.ToString(@"hh\:mm"),
                    ValueAtPurchasing = (asset?.PurchaseValue ?? 0).ToString("N0"),
                    CurrentValue = (asset?.PurchaseValue ?? 0).ToString("N0")
                });
            }

            return result;
        }

        var legacyItems = await _context.AccDiscardNotes.AsNoTracking().OrderByDescending(x => x.Date).ToListAsync(cancellationToken);
        return _mapper.Map<List<AccDiscardNoteDto>>(legacyItems);
    }
}
