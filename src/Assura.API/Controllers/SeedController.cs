using Assura.Application.Common.Interfaces;
using Assura.Domain.Constants;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assura.API.Controllers;

// [Authorize(Roles = Roles.Admin)] // Temporarily disabled for testing
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

    [HttpPost("categories")]
    public async Task<IActionResult> SeedCategories()
    {
        try
        {
            if (await _context.Categories.AnyAsync())
            {
                return BadRequest("Categories already exist.");
            }

            var categories = new List<Category>
            {
                new() { Name = "Computers", Description = "Laptops, Desktops, Servers" },
                new() { Name = "Furniture", Description = "Desks, Chairs, Tables" },
                new() { Name = "Networking", Description = "Routers, Switches, Cables" },
                new() { Name = "Servers", Description = "Servers" },
                new() { Name = "Printing", Description = "Printers" }
            };

            _context.Categories.AddRange(categories);
            await _context.SaveChangesAsync(default);

            return Ok("Categories seeded successfully.");
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



    [HttpPost("test-users")]
    public async Task<IActionResult> SeedTestUsers()
    {
        try
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password@123");

            var admin = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
            if (admin != null)
            {
                admin.PasswordHash = passwordHash;
                admin.Role = UserRole.Admin;
                admin.IsActive = true;
                _context.Users.Update(admin);
            }
            else
            {
                admin = new User
                {
                    Username = "admin",
                    PasswordHash = passwordHash,
                    Email = "admin@assura.com",
                    FirstName = "System",
                    LastName = "Admin",
                    Role = UserRole.Admin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Users.Add(admin);
            }

            var procurement = await _context.Users.FirstOrDefaultAsync(u => u.Username == "procurement");
            if (procurement != null)
            {
                procurement.PasswordHash = passwordHash;
                procurement.Role = UserRole.Procurement;
                procurement.IsActive = true;
                _context.Users.Update(procurement);
            }
            else
            {
                procurement = new User
                {
                    Username = "procurement",
                    PasswordHash = passwordHash,
                    Email = "proc@assura.com",
                    FirstName = "Procurement",
                    LastName = "Officer",
                    Role = UserRole.Procurement,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Users.Add(procurement);
            }

            var auditor = await _context.Users.FirstOrDefaultAsync(u => u.Username == "auditor");
            if (auditor != null)
            {
                auditor.PasswordHash = passwordHash;
                auditor.Role = UserRole.Auditor;
                auditor.IsActive = true;
                _context.Users.Update(auditor);
            }
            else
            {
                auditor = new User
                {
                    Username = "auditor",
                    PasswordHash = passwordHash,
                    Email = "auditor@assura.com",
                    FirstName = "System",
                    LastName = "Auditor",
                    Role = UserRole.Auditor,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Users.Add(auditor);
            }

            await _context.SaveChangesAsync(default);

            return Ok("Test users updated/seeded successfully with password: Password@123");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    [HttpPost("suppliers")]
    public async Task<IActionResult> SeedSuppliers()
    {
        try
        {
            if (await _context.Suppliers.AnyAsync())
            {
                return BadRequest("Suppliers already exist.");
            }

            var suppliers = new List<Supplier>
            {
                new() { Name = "Super Neat technology (Pvt)Ltd.", Phone = "0114814646", Email = "sales4@superneat.lk", Address = "No 478, Kandy road, Kelaniya", CreatedAt = DateTime.UtcNow },
                new() { Name = "TechWave Solutions Inc.", Phone = "0112345678", Website = "www.techwave.lk", Email = "info@techwave.lk", Address = "No 22, Galle Road, Colombo", CreatedAt = DateTime.UtcNow },
                new() { Name = "Global Systems & Services", Phone = "0119876543", Website = "www.globalsys.lk", Email = "contact@globalsys.lk", Address = "No 56, Hospital Road, Kandy", CreatedAt = DateTime.UtcNow },
                new() { Name = "Apex Procurement Co.", Phone = "0117654321", Email = "apex@procurement.lk", Address = "No 10, Main Street, Galle", CreatedAt = DateTime.UtcNow },
                new() { Name = "NovaTech Industries Ltd.", Phone = "0115432109", Website = "www.novatech.lk", Email = "nova@novatech.lk", Address = "No 88, Industrial Zone, Ratmalana", CreatedAt = DateTime.UtcNow }
            };

            _context.Suppliers.AddRange(suppliers);
            await _context.SaveChangesAsync(default);

            return Ok("Suppliers seeded successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("transfers")]
    public async Task<IActionResult> SeedTransfers()
    {
        try
        {
            // Get existing data or create minimal required data
            var divisions = await _context.Divisions.ToListAsync();
            if (!divisions.Any())
            {
                return BadRequest("Please seed divisions first.");
            }

            var users = await _context.Users.ToListAsync();
            if (!users.Any())
            {
                return BadRequest("Please seed test users first.");
            }

            // Create sample products and assets if they don't exist
            var products = await _context.Products.ToListAsync();
            if (!products.Any())
            {
                var sampleProducts = new List<Product>
                {
                    new() { Name = "Laptop Dell XPS 15", CreatedAt = DateTime.UtcNow },
                    new() { Name = "Office Chair Ergonomic", CreatedAt = DateTime.UtcNow },
                    new() { Name = "Network Switch 24 Port", CreatedAt = DateTime.UtcNow }
                };
                _context.Products.AddRange(sampleProducts);
                await _context.SaveChangesAsync(default);
                products = await _context.Products.ToListAsync();
            }

            var assets = await _context.Assets.ToListAsync();
            if (!assets.Any())
            {
                var sampleAssets = new List<Asset>
                {
                    new() { AssetTag = "LAP001", ProductId = products[0].Id, DivisionId = divisions[0].Id, AssignedUserId = users[0].Id, CreatedAt = DateTime.UtcNow },
                    new() { AssetTag = "CHR001", ProductId = products[1].Id, DivisionId = divisions[1].Id, AssignedUserId = users[1].Id, CreatedAt = DateTime.UtcNow },
                    new() { AssetTag = "SWT001", ProductId = products[2].Id, DivisionId = divisions[2].Id, AssignedUserId = users[2].Id, CreatedAt = DateTime.UtcNow }
                };
                _context.Assets.AddRange(sampleAssets);
                await _context.SaveChangesAsync(default);
                assets = await _context.Assets.ToListAsync();
            }

            // Create sample transfers with different statuses
            var transfers = new List<Transfer>
            {
                new() { 
                    TransferNumber = "TRF-0001", 
                    AssetId = assets[0].Id, 
                    FromDivisionId = divisions[0].Id, 
                    ToDivisionId = divisions[1].Id, 
                    TargetUserId = users[1].Id, 
                    TransferById = users[0].Id,
                    Reason = "Project requirement", 
                    TransferDate = DateTime.Now.AddDays(-5), 
                    ReturnDate = DateTime.Now.AddDays(30), 
                    Status = TransferStatus.PendingOwnerApproval,
                    CreatedAt = DateTime.Now.AddDays(-5)
                },
                new() { 
                    TransferNumber = "TRF-0002", 
                    AssetId = assets[1].Id, 
                    FromDivisionId = divisions[1].Id, 
                    ToDivisionId = divisions[2].Id, 
                    TargetUserId = users[2].Id, 
                    TransferById = users[1].Id,
                    Reason = "Temporary assignment", 
                    TransferDate = DateTime.Now.AddDays(-3), 
                    ReturnDate = DateTime.Now.AddDays(25), 
                    Status = TransferStatus.PendingOwnerDivisionHeadApproval,
                    CreatedAt = DateTime.Now.AddDays(-3)
                },
                new() { 
                    TransferNumber = "TRF-0003", 
                    AssetId = assets[2].Id, 
                    FromDivisionId = divisions[2].Id, 
                    ToDivisionId = divisions[0].Id, 
                    TargetUserId = users[0].Id, 
                    TransferById = users[2].Id,
                    Reason = "Equipment maintenance", 
                    TransferDate = DateTime.Now.AddDays(-1), 
                    ReturnDate = DateTime.Now.AddDays(20), 
                    Status = TransferStatus.WaitingForFinalConfirmation,
                    CreatedAt = DateTime.Now.AddDays(-1)
                }
            };

            _context.Transfers.AddRange(transfers);
            await _context.SaveChangesAsync(default);

            return Ok("Sample transfers seeded successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                Message = ex.Message, 
                InnerMessage = ex.InnerException?.Message,
                StackTrace = ex.StackTrace 
            });
        }
    }
}
