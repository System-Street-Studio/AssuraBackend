<<<<<<< HEAD
using Assura.Application.Common.Interfaces;
using Assura.Domain.Constants;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assura.API.Controllers;

// [Authorize(Roles = Roles.Admin)] // Temporarily disabled for testing
=======
[..SNIP..]
[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
<<<<<<< HEAD
    private readonly IApplicationDbContext _context;

    public SeedController(IApplicationDbContext context)
=======
    private readonly AppDbContext _context;

    public SeedController(AppDbContext context)
>>>>>>> feature/system-updates-dev
    {
        _context = context;
    }

<<<<<<< HEAD
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
                using Assura.Application.Common.Interfaces;
                using Assura.Domain.Constants;
                using Assura.Domain.Entities;
                using Assura.Domain.Enums;
                using Microsoft.AspNetCore.Authorization;
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

                    // ─── POST api/seed/acc-pending-items ───────────────────────────────────────
                    [HttpPost("acc-pending-items")]
                    public async Task<IActionResult> SeedAccPendingItems()
                    {
                        if (await _context.AccPendingItems.AnyAsync())
                        {
                            _context.AccPendingItems.RemoveRange(_context.AccPendingItems);
                            await _context.SaveChangesAsync();
                        }

                        var items = new List<AccPendingItem>
                        {
                            // 5 Approved
                            new AccPendingItem { Name = "Dell Ultrasharp Monitor",  Division = "Design",      Date = DateTime.UtcNow.AddDays(-10), Status = "Approved",       Category = AccPendingCategory.Approved,     Time = new TimeSpan(14,15,0), AssetType = "Monitor",     CurrentUser = "Jane Smith",   SpecialNote = "Color accuracy is gone.",         ValueAtPurchasing = 800m,  CurrentValue = 150m },
                            new AccPendingItem { Name = "Logitech MX Master 3",     Division = "Design",      Date = DateTime.UtcNow.AddDays(-9),  Status = "Approved",       Category = AccPendingCategory.Approved,     Time = new TimeSpan(10, 0,0), AssetType = "Peripheral",  CurrentUser = "Jane Smith",   SpecialNote = "Scroll wheel broken.",            ValueAtPurchasing = 100m,  CurrentValue = 0m },
                            new AccPendingItem { Name = "Standing Desk",            Division = "HR",          Date = DateTime.UtcNow.AddDays(-8),  Status = "Approved",       Category = AccPendingCategory.Approved,     Time = new TimeSpan( 9,30,0), AssetType = "Furniture",   CurrentUser = "Alice Brown",  SpecialNote = "Motor burnt out.",                ValueAtPurchasing = 500m,  CurrentValue = 50m },
                            new AccPendingItem { Name = "Epson Projector",          Division = "Sales",       Date = DateTime.UtcNow.AddDays(-7),  Status = "Approved",       Category = AccPendingCategory.Approved,     Time = new TimeSpan(11, 0,0), AssetType = "Electronics", CurrentUser = "Bob Wilson",   SpecialNote = "Lamp needs replacement.",         ValueAtPurchasing = 1500m, CurrentValue = 200m },
                            new AccPendingItem { Name = "iPad Pro 12.9",            Division = "Marketing",   Date = DateTime.UtcNow.AddDays(-6),  Status = "Approved",       Category = AccPendingCategory.Approved,     Time = new TimeSpan(15,20,0), AssetType = "Tablet",      CurrentUser = "Sarah Connor", SpecialNote = "Cracked screen.",                 ValueAtPurchasing = 1100m, CurrentValue = 100m },

                            // 6 To Be Approved
                            new AccPendingItem { Name = "Office Chair",             Division = "HR",          Date = DateTime.UtcNow.AddDays(-5),  Status = "To Be Approved", Category = AccPendingCategory.ToBeApproved, Time = new TimeSpan( 9, 0,0), AssetType = "Furniture",   CurrentUser = "Alice Brown",  SpecialNote = "Wheel broken.",                   ValueAtPurchasing = 200m,  CurrentValue = 20m },
                            new AccPendingItem { Name = "Lenovo ThinkPad Dock",     Division = "Engineering", Date = DateTime.UtcNow.AddDays(-5),  Status = "To Be Approved", Category = AccPendingCategory.ToBeApproved, Time = new TimeSpan(13,45,0), AssetType = "Peripheral",  CurrentUser = "John Doe",     SpecialNote = "Ports not working.",              ValueAtPurchasing = 250m,  CurrentValue = 30m },
                            new AccPendingItem { Name = "Cisco IP Phone",           Division = "Sales",       Date = DateTime.UtcNow.AddDays(-4),  Status = "To Be Approved", Category = AccPendingCategory.ToBeApproved, Time = new TimeSpan( 8,15,0), AssetType = "Electronics", CurrentUser = "Bob Wilson",   SpecialNote = "No dial tone.",                   ValueAtPurchasing = 150m,  CurrentValue = 10m },
                            new AccPendingItem { Name = "Sony A7III Camera",        Division = "Marketing",   Date = DateTime.UtcNow.AddDays(-4),  Status = "To Be Approved", Category = AccPendingCategory.ToBeApproved, Time = new TimeSpan(14,30,0), AssetType = "Camera",      CurrentUser = "Sarah Connor", SpecialNote = "Sensor damaged.",                 ValueAtPurchasing = 2000m, CurrentValue = 400m },
                            new AccPendingItem { Name = "APC UPS 1500",             Division = "IT",          Date = DateTime.UtcNow.AddDays(-3),  Status = "To Be Approved", Category = AccPendingCategory.ToBeApproved, Time = new TimeSpan(16, 0,0), AssetType = "Electronics", CurrentUser = "Admin",        SpecialNote = "Battery completely dead.",        ValueAtPurchasing = 300m,  CurrentValue = 25m },
                            new AccPendingItem { Name = "Conference Table",         Division = "HR",          Date = DateTime.UtcNow.AddDays(-3),  Status = "To Be Approved", Category = AccPendingCategory.ToBeApproved, Time = new TimeSpan(10,10,0), AssetType = "Furniture",   CurrentUser = "Alice Brown",  SpecialNote = "Deep scratches.",                 ValueAtPurchasing = 1200m, CurrentValue = 300m },

                            // 4 Pending
                            new AccPendingItem { Name = "MacBook Pro 16",           Division = "Engineering", Date = DateTime.UtcNow.AddDays(-2),  Status = "Pending",        Category = AccPendingCategory.Pending,      Time = new TimeSpan(10,30,0), AssetType = "Laptop",      CurrentUser = "John Doe",     SpecialNote = "Screen broken, beyond repair.",   ValueAtPurchasing = 2500m, CurrentValue = 0m,   IsHighlighted = true },
                            new AccPendingItem { Name = "Dell XPS 15",              Division = "Engineering", Date = DateTime.UtcNow.AddDays(-2),  Status = "Pending",        Category = AccPendingCategory.Pending,      Time = new TimeSpan(11,20,0), AssetType = "Laptop",      CurrentUser = "Jane Doe",     SpecialNote = "Motherboard shorted.",            ValueAtPurchasing = 2200m, CurrentValue = 100m, IsHighlighted = true },
                            new AccPendingItem { Name = "HP LaserJet Printer",      Division = "Finance",     Date = DateTime.UtcNow.AddDays(-1),  Status = "Pending",        Category = AccPendingCategory.Pending,      Time = new TimeSpan(14,50,0), AssetType = "Printer",     CurrentUser = "Tom Hanks",    SpecialNote = "Paper jam mechanism broken.",     ValueAtPurchasing = 600m,  CurrentValue = 50m },
                            new AccPendingItem { Name = "Nvidia RTX 3090",          Division = "Design",      Date = DateTime.UtcNow.AddDays(-1),  Status = "Pending",        Category = AccPendingCategory.Pending,      Time = new TimeSpan( 9,40,0), AssetType = "Component",   CurrentUser = "Jane Smith",   SpecialNote = "Artifacts on screen.",            ValueAtPurchasing = 1500m, CurrentValue = 200m, IsHighlighted = true },

                            // 5 Rejected
                            new AccPendingItem { Name = "ThinkPad T14",             Division = "Sales",       Date = DateTime.UtcNow.AddDays(-15), Status = "Rejected",       Category = AccPendingCategory.Rejected,     Time = new TimeSpan(11,45,0), AssetType = "Laptop",      CurrentUser = "Bob Wilson",   SpecialNote = "Missing keys.",                   ValueAtPurchasing = 1200m, CurrentValue = 300m },
                            new AccPendingItem { Name = "Whiteboard",               Division = "HR",          Date = DateTime.UtcNow.AddDays(-14), Status = "Rejected",       Category = AccPendingCategory.Rejected,     Time = new TimeSpan(13,10,0), AssetType = "Furniture",   CurrentUser = "Alice Brown",  SpecialNote = "Request denied.",                 ValueAtPurchasing = 100m,  CurrentValue = 50m },
                            new AccPendingItem { Name = "Samsung 32\" TV",          Division = "Marketing",   Date = DateTime.UtcNow.AddDays(-13), Status = "Rejected",       Category = AccPendingCategory.Rejected,     Time = new TimeSpan(16,45,0), AssetType = "Electronics", CurrentUser = "Sarah Connor", SpecialNote = "Can be repaired locally.",         ValueAtPurchasing = 400m,  CurrentValue = 150m },
                            new AccPendingItem { Name = "Mechanical Keyboard",      Division = "Engineering", Date = DateTime.UtcNow.AddDays(-12), Status = "Rejected",       Category = AccPendingCategory.Rejected,     Time = new TimeSpan(10, 5,0), AssetType = "Peripheral",  CurrentUser = "John Doe",     SpecialNote = "Just needs cleaning.",            ValueAtPurchasing = 150m,  CurrentValue = 100m },
                            new AccPendingItem { Name = "Filing Cabinet",           Division = "Finance",     Date = DateTime.UtcNow.AddDays(-11), Status = "Rejected",       Category = AccPendingCategory.Rejected,     Time = new TimeSpan(15,30,0), AssetType = "Furniture",   CurrentUser = "Tom Hanks",    SpecialNote = "Denied.",                         ValueAtPurchasing = 250m,  CurrentValue = 150m }
                        };

                        _context.AccPendingItems.AddRange(items);
                        await _context.SaveChangesAsync();
                        return Ok($"Seeded {items.Count} AccPendingItems (Approved:5, ToBeApproved:6, Pending:4, Rejected:5).");
                    }

                    // ─── POST api/seed/receipts ────────────────────────────────────────────────
                    [HttpPost("receipts")]
                    public async Task<IActionResult> SeedReceipts()
                    {
                        if (await _context.Receipts.AnyAsync())
                        {
                            _context.Receipts.RemoveRange(_context.Receipts);
                            await _context.SaveChangesAsync();
                        }

                        var receipts = new List<Receipt>
                        {
                            new Receipt { AssetName = "MacBook Pro 16",       Division = "Engineering", Date = DateTime.UtcNow.AddDays(-20), Amount = "2500.00",  Status = ReceiptStatus.Uploaded },
                            new Receipt { AssetName = "Dell XPS 15",          Division = "Engineering", Date = DateTime.UtcNow.AddDays(-18), Amount = "2200.00",  Status = ReceiptStatus.Uploaded },
                            new Receipt { AssetName = "Epson Projector",      Division = "Sales",       Date = DateTime.UtcNow.AddDays(-15), Amount = "1500.00",  Status = ReceiptStatus.Uploaded },
                            new Receipt { AssetName = "Sony A7III Camera",    Division = "Marketing",   Date = DateTime.UtcNow.AddDays(-12), Amount = "2000.00",  Status = ReceiptStatus.Uploaded },
                            new Receipt { AssetName = "iPad Pro 12.9",        Division = "Marketing",   Date = DateTime.UtcNow.AddDays(-10), Amount = "1100.00",  Status = ReceiptStatus.Pending  },
                            new Receipt { AssetName = "APC UPS 1500",         Division = "IT",          Date = DateTime.UtcNow.AddDays(-8),  Amount = "300.00",   Status = ReceiptStatus.Pending  },
                            new Receipt { AssetName = "Conference Table",     Division = "HR",          Date = DateTime.UtcNow.AddDays(-7),  Amount = "1200.00",  Status = ReceiptStatus.Uploaded },
                            new Receipt { AssetName = "Nvidia RTX 3090",      Division = "Design",      Date = DateTime.UtcNow.AddDays(-5),  Amount = "1500.00",  Status = ReceiptStatus.Pending  },
                            new Receipt { AssetName = "HP LaserJet Printer",  Division = "Finance",     Date = DateTime.UtcNow.AddDays(-3),  Amount = "600.00",   Status = ReceiptStatus.Pending  },
                            new Receipt { AssetName = "Cisco IP Phone",       Division = "Sales",       Date = DateTime.UtcNow.AddDays(-2),  Amount = "150.00",   Status = ReceiptStatus.Uploaded },
                        };

                        _context.Receipts.AddRange(receipts);
                        await _context.SaveChangesAsync();
                        return Ok($"Seeded {receipts.Count} Receipts.");
                    }

                    // ─── POST api/seed/lost-items ──────────────────────────────────────────────
                    [HttpPost("lost-items")]
                    public async Task<IActionResult> SeedLostItems()
                    {
                        if (await _context.LostItems.AnyAsync())
                        {
                            _context.LostItems.RemoveRange(_context.LostItems);
                            await _context.SaveChangesAsync();
                        }

                        var lost = new List<LostItem>
                        {
                            new LostItem { AssetName = "Dell Wireless Mouse",    Division = "Design",      Date = DateTime.UtcNow.AddDays(-30), ReportedBy = "Jane Smith",   Status = LostItemStatus.ConfirmedLost,      AssetType = "Peripheral",  Time = new TimeSpan(10, 0,0), ValueAtPurchasing = 80m,   CurrentValue = 30m,  Description = "Last seen on 3rd floor, Design dept." },
                            new LostItem { AssetName = "iPhone 13 Pro",          Division = "Sales",       Date = DateTime.UtcNow.AddDays(-25), ReportedBy = "Bob Wilson",   Status = LostItemStatus.UnderInvestigation, AssetType = "Mobile",      Time = new TimeSpan(14,30,0), ValueAtPurchasing = 1000m, CurrentValue = 500m, Description = "May have been left at client site." },
                            new LostItem { AssetName = "USB-C Hub",              Division = "IT",          Date = DateTime.UtcNow.AddDays(-20), ReportedBy = "Admin",        Status = LostItemStatus.ConfirmedLost,      AssetType = "Peripheral",  Time = new TimeSpan( 9,15,0), ValueAtPurchasing = 60m,   CurrentValue = 0m,   Description = "Missing from server room." },
                            new LostItem { AssetName = "Canon DSLR Camera",      Division = "Marketing",   Date = DateTime.UtcNow.AddDays(-18), ReportedBy = "Sarah Connor", Status = LostItemStatus.UnderInvestigation, AssetType = "Camera",      Time = new TimeSpan(11,45,0), ValueAtPurchasing = 1500m, CurrentValue = 800m, Description = "Last used at company event." },
                            new LostItem { AssetName = "Surface Pro 9",          Division = "Engineering", Date = DateTime.UtcNow.AddDays(-15), ReportedBy = "John Doe",     Status = LostItemStatus.Reported,           AssetType = "Tablet",      Time = new TimeSpan(16, 0,0), ValueAtPurchasing = 1800m, CurrentValue = 900m, Description = "Not found after office relocation." },
                            new LostItem { AssetName = "Logitech Webcam C920",   Division = "HR",          Date = DateTime.UtcNow.AddDays(-10), ReportedBy = "Alice Brown",  Status = LostItemStatus.ConfirmedLost,      AssetType = "Peripheral",  Time = new TimeSpan(13, 0,0), ValueAtPurchasing = 120m,  CurrentValue = 0m,   Description = "Reported missing from meeting room." },
                            new LostItem { AssetName = "Lenovo ThinkBook 14",    Division = "Finance",     Date = DateTime.UtcNow.AddDays(-8),  ReportedBy = "Tom Hanks",    Status = LostItemStatus.UnderInvestigation, AssetType = "Laptop",      Time = new TimeSpan( 8,30,0), ValueAtPurchasing = 1300m, CurrentValue = 700m, Description = "Left in taxi after offsite meeting." },
                            new LostItem { AssetName = "Bluetooth Speaker",      Division = "Design",      Date = DateTime.UtcNow.AddDays(-5),  ReportedBy = "Jane Smith",   Status = LostItemStatus.Reported,           AssetType = "Electronics", Time = new TimeSpan(15,20,0), ValueAtPurchasing = 200m,  CurrentValue = 100m, Description = "Missing from design studio." },
                        };

                        _context.LostItems.AddRange(lost);
                        await _context.SaveChangesAsync();
                        return Ok($"Seeded {lost.Count} LostItems.");
                    }

                    // ─── POST api/seed/queue-items ─────────────────────────────────────────────
                    [HttpPost("queue-items")]
                    public async Task<IActionResult> SeedQueueItems()
                    {
                        if (await _context.QueueItems.AnyAsync())
                        {
                            _context.QueueItems.RemoveRange(_context.QueueItems);
                            await _context.SaveChangesAsync();
                        }

                        var items = new List<QueueItem>
                        {
                            // 5 Pending
                            new QueueItem { Name = "MacBook Pro 16", Division = "Engineering", Date = DateTime.UtcNow.AddDays(-1), Status = QueueItemStatus.Pending, Time = new TimeSpan(10, 0, 0), AssetType = "Laptop", SpecialNote = "Need it urgently" },
                            new QueueItem { Name = "Dell Ultrasharp Monitor", Division = "Design", Date = DateTime.UtcNow.AddDays(-1), Status = QueueItemStatus.Pending, Time = new TimeSpan(11, 0, 0), AssetType = "Monitor", SpecialNote = "" },
                            new QueueItem { Name = "Logitech MX Master 3", Division = "HR", Date = DateTime.UtcNow.AddDays(-1), Status = QueueItemStatus.Pending, Time = new TimeSpan(12, 0, 0), AssetType = "Peripheral", SpecialNote = "" },
                            new QueueItem { Name = "Ergonomic Office Chair", Division = "Sales", Date = DateTime.UtcNow.AddDays(-1), Status = QueueItemStatus.Pending, Time = new TimeSpan(13, 0, 0), AssetType = "Furniture", SpecialNote = "" },
                            new QueueItem { Name = "Standing Desk", Division = "IT", Date = DateTime.UtcNow.AddDays(-1), Status = QueueItemStatus.Pending, Time = new TimeSpan(14, 0, 0), AssetType = "Furniture", SpecialNote = "" },

                            // 4 Discarded
                            new QueueItem { Name = "Old Lenovo ThinkPad", Division = "Engineering", Date = DateTime.UtcNow.AddDays(-2), Status = QueueItemStatus.Discarded, Time = new TimeSpan(9, 0, 0), AssetType = "Laptop", SpecialNote = "Broken beyond repair" },
                            new QueueItem { Name = "Broken Chair", Division = "HR", Date = DateTime.UtcNow.AddDays(-2), Status = QueueItemStatus.Discarded, Time = new TimeSpan(10, 0, 0), AssetType = "Furniture", SpecialNote = "Leg snapped" },
                            new QueueItem { Name = "Dead Dell Monitor", Division = "IT", Date = DateTime.UtcNow.AddDays(-2), Status = QueueItemStatus.Discarded, Time = new TimeSpan(11, 0, 0), AssetType = "Monitor", SpecialNote = "Doesn't power on" },
                            new QueueItem { Name = "Faulty Mouse", Division = "Sales", Date = DateTime.UtcNow.AddDays(-2), Status = QueueItemStatus.Discarded, Time = new TimeSpan(12, 0, 0), AssetType = "Peripheral", SpecialNote = "Double clicks" },

                            // 4 Unread
                            new QueueItem { Name = "iPad Pro", Division = "Marketing", Date = DateTime.UtcNow, Status = QueueItemStatus.Unread, Time = new TimeSpan(8, 0, 0), AssetType = "Tablet", SpecialNote = "For new campaign" },
                            new QueueItem { Name = "Sony Headphones", Division = "Engineering", Date = DateTime.UtcNow, Status = QueueItemStatus.Unread, Time = new TimeSpan(9, 0, 0), AssetType = "Peripheral", SpecialNote = "Noise cancelling" },
                            new QueueItem { Name = "Logitech Webcam", Division = "HR", Date = DateTime.UtcNow, Status = QueueItemStatus.Unread, Time = new TimeSpan(10, 0, 0), AssetType = "Peripheral", SpecialNote = "For remote interviews" },
                            new QueueItem { Name = "Whiteboard Markers", Division = "Design", Date = DateTime.UtcNow, Status = QueueItemStatus.Unread, Time = new TimeSpan(11, 0, 0), AssetType = "Stationery", SpecialNote = "Run out of ink" },

                            // 5 Rejected
                            new QueueItem { Name = "Gaming PC", Division = "Engineering", Date = DateTime.UtcNow.AddDays(-3), Status = QueueItemStatus.Rejected, Time = new TimeSpan(14, 0, 0), AssetType = "Desktop", SpecialNote = "Not approved for work" },
                            new QueueItem { Name = "Herman Miller Chair", Division = "Sales", Date = DateTime.UtcNow.AddDays(-3), Status = QueueItemStatus.Rejected, Time = new TimeSpan(15, 0, 0), AssetType = "Furniture", SpecialNote = "Over budget" },
                            new QueueItem { Name = "85 inch 4K TV", Division = "Marketing", Date = DateTime.UtcNow.AddDays(-3), Status = QueueItemStatus.Rejected, Time = new TimeSpan(16, 0, 0), AssetType = "Electronics", SpecialNote = "Not needed" },
                            new QueueItem { Name = "Custom Mechanical Keyboard", Division = "IT", Date = DateTime.UtcNow.AddDays(-3), Status = QueueItemStatus.Rejected, Time = new TimeSpan(17, 0, 0), AssetType = "Peripheral", SpecialNote = "Too expensive" },
                            new QueueItem { Name = "Samsung Galaxy Tab", Division = "HR", Date = DateTime.UtcNow.AddDays(-3), Status = QueueItemStatus.Rejected, Time = new TimeSpan(18, 0, 0), AssetType = "Tablet", SpecialNote = "Use existing ones" },

                            // 4 Approved
                            new QueueItem { Name = "Ergonomic Mouse", Division = "Engineering", Date = DateTime.UtcNow.AddDays(-4), Status = QueueItemStatus.Approved, Time = new TimeSpan(9, 0, 0), AssetType = "Peripheral", SpecialNote = "" },
                            new QueueItem { Name = "Large Whiteboard", Division = "HR", Date = DateTime.UtcNow.AddDays(-4), Status = QueueItemStatus.Approved, Time = new TimeSpan(10, 0, 0), AssetType = "Furniture", SpecialNote = "" },
                            new QueueItem { Name = "Epson Projector", Division = "Sales", Date = DateTime.UtcNow.AddDays(-4), Status = QueueItemStatus.Approved, Time = new TimeSpan(11, 0, 0), AssetType = "Electronics", SpecialNote = "" },
                            new QueueItem { Name = "Dell Docking Station", Division = "IT", Date = DateTime.UtcNow.AddDays(-4), Status = QueueItemStatus.Approved, Time = new TimeSpan(12, 0, 0), AssetType = "Peripheral", SpecialNote = "" }
                        };

                        _context.QueueItems.AddRange(items);
                        await _context.SaveChangesAsync();
                        return Ok($"Seeded {items.Count} QueueItems.");
                    }

                    // ─── POST api/seed/all ─────────────────────────────────────────────────────
                    [HttpPost("all")]
                    public async Task<IActionResult> SeedAll()
                    {
                        await SeedAccPendingItems();
                        await SeedReceipts();
                        await SeedLostItems();
                        await SeedQueueItems();
                        return Ok("Seeded AccPendingItems, Receipts, LostItems, and QueueItems successfully.");
                    }
                }
