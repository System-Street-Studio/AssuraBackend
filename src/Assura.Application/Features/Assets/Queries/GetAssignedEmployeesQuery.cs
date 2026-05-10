using MediatR;
using Assura.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using Assura.Application.Common.Interfaces;

namespace Assura.Application.Features.Assets.Queries;

public record GetAssignedEmployeesQuery : IRequest<List<AssignedEmployeeDto>>;

public class GetAssignedEmployeesQueryHandler : IRequestHandler<GetAssignedEmployeesQuery, List<AssignedEmployeeDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAssignedEmployeesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AssignedEmployeeDto>> Handle(GetAssignedEmployeesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            Console.WriteLine($"🔍 [GetAssignedEmployeesQueryHandler] Retrieving unique employees with assigned assets...");

            var employees = await _context.Assets
                .AsNoTracking()
                .Where(a => a.AssignedUserId != null)
                .Select(a => new
                {
                    Id = a.AssignedUserId.Value,
                    FirstName = a.AssignedUser.FirstName,
                    LastName = a.AssignedUser.LastName
                })
                .Distinct()
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToListAsync(cancellationToken);

            Console.WriteLine($"  ✓ Found {employees.Count} unique employees with assigned assets");

            var result = employees.Select(e => new AssignedEmployeeDto
            {
                Id = e.Id,
                Name = $"{e.FirstName} {e.LastName}".Trim()
            })
            .OrderBy(e => e.Name)
            .ToList();

            Console.WriteLine($"✅ [GetAssignedEmployeesQueryHandler] Successfully retrieved {result.Count} employees");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [GetAssignedEmployeesQueryHandler] Error: {ex.Message}");
            Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}
