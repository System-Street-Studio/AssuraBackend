using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assura.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public SeedController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("divisions")]
    public async Task<IActionResult> SeedDivisions()
    {
        try
        {
            if (await _context.Divisions.AnyAsync())
            {
                return BadRequest("Divisions already exist.");
            }

            var divisions = new List<Division>
            {
                new() { Name = "Information Technology" },
                new() { Name = "Industrial Services" },
                new() { Name = "Electronics and Microelectronics" },
                new() { Name = "Communication Engineering" },
                new() { Name = "Space Applications" },
                new() { Name = "Astronomy" },
                new() { Name = "Admin" },
                new() { Name = "Finance" },
                new() { Name = "Procurement" },
                new() { Name = "Stores" },
                new() { Name = "Human Resource" }
            };

            _context.Divisions.AddRange(divisions);
            await _context.SaveChangesAsync(default);

            return Ok("Divisions seeded successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new 
            { 
                Message = ex.Message, 
                InnerMessage = ex.InnerException?.Message,
                StackTrace = ex.StackTrace 
            });
        }
    }
}
