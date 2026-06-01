using Assura.Domain.Entities;
using Assura.Domain.Enums;
using Assura.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace Assura.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
    private readonly AppDbContext _context;

    public SeedController(AppDbContext context)
    {
        _context = context;
    }

    // ─── POST api/seed/acc-pending-items ───────────────────────────────────────
    [HttpPost("acc-pending-items")]
    public async Task<IActionResult> SeedAccPendingItems()
    {
        if (_context.AccPendingItems.Any())
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
        if (_context.Receipts.Any())
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
        if (_context.LostItems.Any())
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

    // ─── POST api/seed/all ─────────────────────────────────────────────────────
    [HttpPost("all")]
    public async Task<IActionResult> SeedAll()
    {
        await SeedAccPendingItems();
        await SeedReceipts();
        await SeedLostItems();
        return Ok("Seeded AccPendingItems, Receipts, and LostItems successfully.");
    }
}
