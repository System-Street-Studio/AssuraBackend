using MediatR;
using Assura.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using Assura.Application.Common.Interfaces;

namespace Assura.Application.Features.Assets.Queries;

public record GetAssignedDivisionsQuery : IRequest<List<DivisionDto>>;

public class GetAssignedDivisionsQueryHandler : IRequestHandler<GetAssignedDivisionsQuery, List<DivisionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAssignedDivisionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DivisionDto>> Handle(GetAssignedDivisionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
           
            // Retrieve unique divisions that have assigned assets
            
            var divisions = await _context.Assets
                .AsNoTracking()
                .Where(a => a.AssignedUserId != null && a.Division != null)
                .Select(a => new
                {
                    Id = a.Division.Id,
                    Name = a.Division.Name
                })
                .Distinct()      // Ensure we only get unique divisions
                .OrderBy(d => d.Name)
                .ToListAsync(cancellationToken);


            var result = divisions.Select(d => new DivisionDto
            {
                Id = d.Id,
                Name = d.Name
            })
            .ToList();
            
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($" [GetAssignedDivisionsQueryHandler] Error: {ex.Message}");
       
            throw;
        }
    }
}
