using Assura.Application.Common.Interfaces;
using Assura.Domain.Constants;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assura.API.Controllers;

// These are dev/test data-seeding utilities, not application features — every action mutates
// or resets data (including resetting the admin/sysadmin passwords to hardcoded defaults and
// running raw SQL updates), so the whole controller is restricted to Admin/SystemAdmin.
// A fresh deployment can still reach this: DbInitializer.SeedAsync bootstraps a default Admin
// account (with a randomly generated, logged-once password) if none exists yet.
[Authorize(Roles = $"{Roles.Admin},{Roles.SystemAdmin}")]
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
        if (await _context.Divisions.AnyAsync()) return BadRequest("Divisions already exist.");

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

    [HttpPost("categories")]
    public async Task<IActionResult> SeedCategories()
    {
        if (await _context.Categories.AnyAsync()) return BadRequest("Categories already exist.");

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

    [HttpPost("test-users")]
    public async Task<IActionResult> SeedTestUsers()
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password@123");

        var admin = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
        if (admin != null)
        {
            admin.PasswordHash = passwordHash; admin.Role = UserRole.Admin; admin.IsActive = true; _context.Users.Update(admin);
        }
        else
        {
            admin = new User { Username = "admin", PasswordHash = passwordHash, Email = "admin@assura.com", FirstName = "System", LastName = "Admin", Role = UserRole.Admin, IsActive = true, CreatedAt = DateTime.UtcNow };
            _context.Users.Add(admin);
        }

        var procurement = await _context.Users.FirstOrDefaultAsync(u => u.Username == "procurement");
        if (procurement != null) { procurement.PasswordHash = passwordHash; procurement.Role = UserRole.Procurement; procurement.IsActive = true; _context.Users.Update(procurement); }
        else { procurement = new User { Username = "procurement", PasswordHash = passwordHash, Email = "proc@assura.com", FirstName = "Procurement", LastName = "Officer", Role = UserRole.Procurement, IsActive = true, CreatedAt = DateTime.UtcNow }; _context.Users.Add(procurement); }

        var auditor = await _context.Users.FirstOrDefaultAsync(u => u.Username == "auditor");
        if (auditor != null) { auditor.PasswordHash = passwordHash; auditor.Role = UserRole.Auditor; auditor.IsActive = true; _context.Users.Update(auditor); }
        else { auditor = new User { Username = "auditor", PasswordHash = passwordHash, Email = "auditor@assura.com", FirstName = "System", LastName = "Auditor", Role = UserRole.Auditor, IsActive = true, CreatedAt = DateTime.UtcNow }; _context.Users.Add(auditor); }

        var accountant = await _context.Users.FirstOrDefaultAsync(u => u.Username == "accountant" || u.Email == "accountant@assura.com");
        if (accountant != null) { accountant.Username = "accountant"; accountant.PasswordHash = passwordHash; accountant.Role = UserRole.Accountant; accountant.IsActive = true; _context.Users.Update(accountant); }
        else { accountant = new User { Username = "accountant", PasswordHash = passwordHash, Email = "accountant@assura.com", FirstName = "System", LastName = "Accountant", Role = UserRole.Accountant, IsActive = true, CreatedAt = DateTime.UtcNow }; _context.Users.Add(accountant); }

        var sysadminPasswordHash = BCrypt.Net.BCrypt.HashPassword("SysAdmin@123");
        var sysadmin = await _context.Users.FirstOrDefaultAsync(u => u.Username == "sysadmin");
        if (sysadmin != null) { sysadmin.PasswordHash = sysadminPasswordHash; sysadmin.Role = UserRole.SystemAdmin; sysadmin.IsActive = true; _context.Users.Update(sysadmin); }
        else { sysadmin = new User { Username = "sysadmin", PasswordHash = sysadminPasswordHash, Email = "sysadmin@assura.com", FirstName = "System", LastName = "Administrator", Role = UserRole.SystemAdmin, IsActive = true, CreatedAt = DateTime.UtcNow }; _context.Users.Add(sysadmin); }

        await _context.SaveChangesAsync(default);
        return Ok("Test users updated/seeded successfully with password: Password@123");
    }

    [HttpPost("suppliers")]
    public async Task<IActionResult> SeedSuppliers()
    {
        if (await _context.Suppliers.AnyAsync()) return BadRequest("Suppliers already exist.");

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

    [HttpPost("transfers")]
    public async Task<IActionResult> SeedTransfers()
    {
        var divisions = await _context.Divisions.ToListAsync(); if (!divisions.Any()) return BadRequest("Please seed divisions first.");
        var users = await _context.Users.ToListAsync(); if (!users.Any()) return BadRequest("Please seed test users first.");

        var products = await _context.Products.ToListAsync();
        if (!products.Any()) { var sampleProducts = new List<Product> { new() { Name = "Laptop Dell XPS 15", CreatedAt = DateTime.UtcNow }, new() { Name = "Office Chair Ergonomic", CreatedAt = DateTime.UtcNow }, new() { Name = "Network Switch 24 Port", CreatedAt = DateTime.UtcNow } }; _context.Products.AddRange(sampleProducts); await _context.SaveChangesAsync(default); products = await _context.Products.ToListAsync(); }

        var assets = await _context.Assets.ToListAsync();
        if (!assets.Any()) { var sampleAssets = new List<Asset> { new() { AssetTag = "LAP001", ProductId = products[0].Id, DivisionId = divisions[0].Id, AssignedUserId = users[0].Id, CreatedAt = DateTime.UtcNow }, new() { AssetTag = "CHR001", ProductId = products[1].Id, DivisionId = divisions[1].Id, AssignedUserId = users[1].Id, CreatedAt = DateTime.UtcNow }, new() { AssetTag = "SWT001", ProductId = products[2].Id, DivisionId = divisions[2].Id, AssignedUserId = users[2].Id, CreatedAt = DateTime.UtcNow } }; _context.Assets.AddRange(sampleAssets); await _context.SaveChangesAsync(default); assets = await _context.Assets.ToListAsync(); }

        var transfers = new List<Transfer>
            {
                new() { TransferNumber = "TRF-0001", AssetId = assets[0].Id, FromDivisionId = divisions[0].Id, ToDivisionId = divisions[1].Id, TargetUserId = users[1].Id, TransferById = users[0].Id, Reason = "Project requirement", TransferDate = DateTime.Now.AddDays(-5), ReturnDate = DateTime.Now.AddDays(30), Status = TransferStatus.PendingOwnerApproval, CreatedAt = DateTime.Now.AddDays(-5) },
                new() { TransferNumber = "TRF-0002", AssetId = assets[1].Id, FromDivisionId = divisions[1].Id, ToDivisionId = divisions[2].Id, TargetUserId = users[2].Id, TransferById = users[1].Id, Reason = "Temporary assignment", TransferDate = DateTime.Now.AddDays(-3), ReturnDate = DateTime.Now.AddDays(25), Status = TransferStatus.PendingOwnerDivisionHeadApproval, CreatedAt = DateTime.Now.AddDays(-3) },
                new() { TransferNumber = "TRF-0003", AssetId = assets[2].Id, FromDivisionId = divisions[2].Id, ToDivisionId = divisions[0].Id, TargetUserId = users[0].Id, TransferById = users[2].Id, Reason = "Equipment maintenance", TransferDate = DateTime.Now.AddDays(-1), ReturnDate = DateTime.Now.AddDays(20), Status = TransferStatus.WaitingForFinalConfirmation, CreatedAt = DateTime.Now.AddDays(-1) }
            };

        _context.Transfers.AddRange(transfers);
        await _context.SaveChangesAsync(default);
        return Ok("Sample transfers seeded successfully.");
    }

    [HttpPost("acc-pending-items")]
    public async Task<IActionResult> SeedAccPendingItems()
    {
        if (await _context.AccPendingItems.AnyAsync()) { _context.AccPendingItems.RemoveRange(_context.AccPendingItems); await _context.SaveChangesAsync(default); }
        var items = new List<AccPendingItem>
        {
                new AccPendingItem { Name = "Dell Ultrasharp Monitor", Division = "Design", Date = DateTime.UtcNow.AddDays(-10), Status = "Approved", Category = AccPendingCategory.Approved, Time = new TimeSpan(14,15,0), AssetType = "Monitor", CurrentUser = "Jane Smith", SpecialNote = "Color accuracy is gone.", ValueAtPurchasing = 800m, CurrentValue = 150m },
                new AccPendingItem { Name = "Logitech MX Master 3", Division = "Design", Date = DateTime.UtcNow.AddDays(-9), Status = "Approved", Category = AccPendingCategory.Approved, Time = new TimeSpan(10,0,0), AssetType = "Peripheral", CurrentUser = "Jane Smith", SpecialNote = "Scroll wheel broken.", ValueAtPurchasing = 100m, CurrentValue = 0m },
                new AccPendingItem { Name = "Standing Desk", Division = "HR", Date = DateTime.UtcNow.AddDays(-8), Status = "Approved", Category = AccPendingCategory.Approved, Time = new TimeSpan(9,30,0), AssetType = "Furniture", CurrentUser = "Alice Brown", SpecialNote = "Motor burnt out.", ValueAtPurchasing = 500m, CurrentValue = 50m },
                new AccPendingItem { Name = "Epson Projector", Division = "Sales", Date = DateTime.UtcNow.AddDays(-7), Status = "Approved", Category = AccPendingCategory.Approved, Time = new TimeSpan(11,0,0), AssetType = "Electronics", CurrentUser = "Bob Wilson", SpecialNote = "Lamp needs replacement.", ValueAtPurchasing = 1500m, CurrentValue = 200m },
                new AccPendingItem { Name = "iPad Pro 12.9", Division = "Marketing", Date = DateTime.UtcNow.AddDays(-6), Status = "Approved", Category = AccPendingCategory.Approved, Time = new TimeSpan(15,20,0), AssetType = "Tablet", CurrentUser = "Sarah Connor", SpecialNote = "Cracked screen.", ValueAtPurchasing = 1100m, CurrentValue = 100m },
            };
        _context.AccPendingItems.AddRange(items); await _context.SaveChangesAsync(default); return Ok($"Seeded {items.Count} AccPendingItems (Approved:5, ToBeApproved:6, Pending:4, Rejected:5).");
    }

    [HttpPost("receipts")]
    public async Task<IActionResult> SeedReceipts()
    {
        if (await _context.Receipts.AnyAsync()) { _context.Receipts.RemoveRange(_context.Receipts); await _context.SaveChangesAsync(default); }
        var receipts = new List<Receipt> { new Receipt { AssetName = "MacBook Pro 16", Division = "Engineering", Date = DateTime.UtcNow.AddDays(-20), Amount = 2500.00m, Status = ReceiptStatus.Uploaded }, new Receipt { AssetName = "Dell XPS 15", Division = "Engineering", Date = DateTime.UtcNow.AddDays(-18), Amount = 2200.00m, Status = ReceiptStatus.Uploaded }, new Receipt { AssetName = "Epson Projector", Division = "Sales", Date = DateTime.UtcNow.AddDays(-15), Amount = 1500.00m, Status = ReceiptStatus.Uploaded } };
        _context.Receipts.AddRange(receipts); await _context.SaveChangesAsync(default); return Ok($"Seeded {receipts.Count} Receipts.");
    }

    [HttpPost("lost-items")]
    public async Task<IActionResult> SeedLostItems()
    {
        if (await _context.LostItems.AnyAsync()) { _context.LostItems.RemoveRange(_context.LostItems); await _context.SaveChangesAsync(default); }
        var lost = new List<LostItem> { new LostItem { AssetName = "Dell Wireless Mouse", Division = "Design", Date = DateTime.UtcNow.AddDays(-30), ReportedBy = "Jane Smith", Status = LostItemStatus.ConfirmedLost, AssetType = "Peripheral", Time = new TimeSpan(10, 0, 0), ValueAtPurchasing = 80m, CurrentValue = 30m, Description = "Last seen on 3rd floor, Design dept." } };
        _context.LostItems.AddRange(lost); await _context.SaveChangesAsync(default); return Ok($"Seeded {lost.Count} LostItems.");
    }

    [HttpPost("queue-items")]
    public async Task<IActionResult> SeedQueueItems()
    {
        if (await _context.QueueItems.AnyAsync()) { _context.QueueItems.RemoveRange(_context.QueueItems); await _context.SaveChangesAsync(default); }
        var items = new List<QueueItem> { new QueueItem { Name = "MacBook Pro 16", Division = "Engineering", Date = DateTime.UtcNow.AddDays(-1), Status = QueueItemStatus.Pending, Time = new TimeSpan(10, 0, 0), AssetType = "Laptop", SpecialNote = "Need it urgently" } };
        _context.QueueItems.AddRange(items); await _context.SaveChangesAsync(default); return Ok($"Seeded {items.Count} QueueItems.");
    }

    [HttpPost("all")]
    public async Task<IActionResult> SeedAll()
    {
        await SeedAccPendingItems();
        await SeedReceipts();
        await SeedLostItems();
        await SeedQueueItems();
        return Ok("Seeded AccPendingItems, Receipts, LostItems, and QueueItems successfully.");
    }
    [HttpGet("fix-null-assets")]
    public async Task<IActionResult> FixNullAssets()
    {
        var dbContext = _context as DbContext;
        if (dbContext != null)
        {
            try
            {
                var catId = await dbContext.Set<Category>().Select(c => c.Id).FirstOrDefaultAsync();
                var divId = await dbContext.Set<Division>().Select(d => d.Id).FirstOrDefaultAsync();
                var prodId = await dbContext.Set<Product>().Select(p => p.Id).FirstOrDefaultAsync();
                var supId = await dbContext.Set<Supplier>().Select(s => s.Id).FirstOrDefaultAsync();

                if (catId > 0) await dbContext.Database.ExecuteSqlRawAsync($"UPDATE Assets SET CategoryId = {catId} WHERE CategoryId IS NULL;");
                if (divId > 0) await dbContext.Database.ExecuteSqlRawAsync($"UPDATE Assets SET DivisionId = {divId} WHERE DivisionId IS NULL;");
                if (prodId > 0) await dbContext.Database.ExecuteSqlRawAsync($"UPDATE Assets SET ProductId = {prodId} WHERE ProductId IS NULL;");
                if (supId > 0) await dbContext.Database.ExecuteSqlRawAsync($"UPDATE Assets SET SupplierId = {supId} WHERE SupplierId IS NULL;");
                await dbContext.Database.ExecuteSqlRawAsync($"UPDATE Assets SET Status = 1 WHERE Status IS NULL;");
                await dbContext.Database.ExecuteSqlRawAsync($"UPDATE Assets SET PurchaseValue = 0 WHERE PurchaseValue IS NULL;");

                return Ok("Successfully cleaned up NULL values in the Assets table.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating Database: {ex.Message}");
            }
        }
        return BadRequest("Could not access Database.");
    }
}
