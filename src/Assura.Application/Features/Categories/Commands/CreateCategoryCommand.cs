using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using Assura.Domain.Entities;
using MediatR;

namespace Assura.Application.Features.Categories.Commands;

public record CreateCategoryCommand(string Name, string? Description) : IRequest<CategoryDto>;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly IApplicationDbContext _context;

    public CreateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = new Category
        {
            Name = request.Name,
            Description = request.Description
        };

        _context.Categories.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new CategoryDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description
        };
    }
}
