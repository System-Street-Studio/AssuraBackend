using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using Assura.Domain.Entities;
using MediatR;

namespace Assura.Application.Features.Divisions.Commands;

public record CreateDivisionCommand(string Name, string? Description) : IRequest<DivisionDto>;

public class CreateDivisionCommandHandler : IRequestHandler<CreateDivisionCommand, DivisionDto>
{
    private readonly IApplicationDbContext _context;

    public CreateDivisionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DivisionDto> Handle(CreateDivisionCommand request, CancellationToken cancellationToken)
    {
        var entity = new Division
        {
            Name = request.Name,
            Description = request.Description
        };

        _context.Divisions.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new DivisionDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description
        };
    }
}
