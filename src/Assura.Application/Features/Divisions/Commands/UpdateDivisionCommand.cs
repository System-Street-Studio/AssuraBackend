using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Divisions.Commands;

public record UpdateDivisionCommand(int Id, string Name, string? Description) : IRequest<DivisionDto>;

public class UpdateDivisionCommandHandler : IRequestHandler<UpdateDivisionCommand, DivisionDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateDivisionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DivisionDto> Handle(UpdateDivisionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Divisions.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new KeyNotFoundException($"Division with ID {request.Id} not found.");
        }

        entity.Name = request.Name;
        entity.Description = request.Description;

        await _context.SaveChangesAsync(cancellationToken);

        return new DivisionDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description
        };
    }
}
