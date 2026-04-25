using MediatR;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;

namespace Assura.Application.Features.DiscardedNotes.Commands.UpdateStatus;

public class UpdateDiscardedNoteStatusCommand : IRequest<bool>
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
}

public class UpdateDiscardedNoteStatusCommandHandler : IRequestHandler<UpdateDiscardedNoteStatusCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateDiscardedNoteStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateDiscardedNoteStatusCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.DiscardedNotes.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null) return false;

        if (Enum.TryParse<DiscardNoteStatus>(request.Status, true, out var status))
        {
            entity.Status = status;
        }

        if (!string.IsNullOrEmpty(request.Note))
        {
            entity.SpecialNote = request.Note;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
