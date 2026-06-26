using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Divisions.Commands;

public record DeleteDivisionCommand(int Id) : IRequest<bool>;

public class DeleteDivisionCommandHandler : IRequestHandler<DeleteDivisionCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteDivisionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteDivisionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Divisions.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new KeyNotFoundException($"Division with ID {request.Id} not found.");
        }

        entity.IsDeleted = true;
        _context.Divisions.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
