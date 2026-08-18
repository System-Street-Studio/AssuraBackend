using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Categories.Commands;

public record UpdateCategoryCommand(int Id, string Name, string? Description, decimal? DepreciationRate = null) : IRequest<CategoryDto>;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Categories.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new KeyNotFoundException($"Category with ID {request.Id} not found.");
        }

        entity.Name = request.Name;
        entity.Description = request.Description;
        if (request.DepreciationRate.HasValue && request.DepreciationRate.Value >= 0)
        {
            entity.DepreciationRate = request.DepreciationRate.Value;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new CategoryDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            DepreciationRate = entity.DepreciationRate
        };
    }
}
