using Assura.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Microsoft.Extensions.Configuration;

namespace Assura.Infrastructure.Persistence;

/// <summary>
/// Seeds the database with default reference data (categories, etc.) on application startup.
/// Only inserts rows that do not already exist (matched by Name), so it is safe to run repeatedly.
/// Also migrates assets from legacy categories to the standard ones and soft-deletes the old entries.
/// </summary>
public static class DbInitializer
{
    private class SeedCategory
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal DepreciationRate { get; set; } = 10.0m;
    }

    /// <summary>
    /// Maps legacy/ad-hoc category names to their standard replacement.
    /// Key = old category name (case-insensitive match), Value = standard category name.
    /// </summary>
    private static readonly Dictionary<string, string> LegacyCategoryMapping = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Computers",   "Computer & Peripherals" },
        { "Laptop",      "Computer & Peripherals" },
        { "Monitor",     "Computer & Peripherals" },
        { "Peripheral",  "Computer & Peripherals" },
        { "Networking",  "Computer & Peripherals" },
        { "Servers",     "Computer & Peripherals" },
        { "Printing",    "Office Equipment" },
        { "Furniture",   "Furniture & Fittings" },
    };

    /// <summary>
    /// Ensures all default categories exist in the database, migrates assets from
    /// legacy categories to the correct standard ones, and soft-deletes the old entries.
    /// Call this from Program.cs after building the application host.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetService<ILogger<AppDbContext>>();
        var config = scope.ServiceProvider.GetService<IConfiguration>();

        var defaultCategories = config?.GetSection("SeedData:Categories").Get<List<SeedCategory>>() ?? new List<SeedCategory>();
        var defaultDivisions = config?.GetSection("SeedData:Divisions").Get<List<string>>() ?? new List<string>();

        try
        {
            // Ensure the database schema is up to date
            await context.Database.MigrateAsync();

            // ── Step 1: Seed standard categories ──
            var allCategories = await context.Categories
                .IgnoreQueryFilters()
                .ToListAsync();

            var existingNames = allCategories.Select(c => c.Name).ToList();

            var toAdd = defaultCategories
                .Where(dc => !existingNames.Contains(dc.Name))
                .Select(dc => new Category
                {
                    Name = dc.Name,
                    Description = dc.Description,
                    DepreciationRate = dc.DepreciationRate > 0 ? dc.DepreciationRate : 10.0m
                })
                .ToList();

            if (toAdd.Count > 0)
            {
                context.Categories.AddRange(toAdd);
                await context.SaveChangesAsync();
                logger?.LogInformation("Seeded {Count} default categories.", toAdd.Count);

                // Refresh the list after adding
                allCategories = await context.Categories
                    .IgnoreQueryFilters()
                    .ToListAsync();
            }

            // Also synchronize default rates for existing categories if unassigned or updated
            bool updatedRates = false;
            foreach (var existingCat in allCategories.Where(c => !c.IsDeleted))
            {
                var matchingSeed = defaultCategories.FirstOrDefault(dc => string.Equals(dc.Name, existingCat.Name, StringComparison.OrdinalIgnoreCase));
                if (matchingSeed != null && matchingSeed.DepreciationRate > 0 && (existingCat.DepreciationRate <= 0 || existingCat.DepreciationRate == 10.0m && matchingSeed.DepreciationRate != 10.0m))
                {
                    existingCat.DepreciationRate = matchingSeed.DepreciationRate;
                    updatedRates = true;
                }
            }
            if (updatedRates)
            {
                await context.SaveChangesAsync();
                logger?.LogInformation("Updated standard depreciation rates for existing categories.");
            }

            // ── Step 2: Reassign assets from legacy categories to standard ones ──
            var standardNames = defaultCategories.Select(dc => dc.Name).ToHashSet();
            var legacyCategories = allCategories
                .Where(c => !c.IsDeleted && !standardNames.Contains(c.Name))
                .ToList();

            if (legacyCategories.Count > 0)
            {
                foreach (var legacy in legacyCategories)
                {
                    // Find the target standard category
                    string targetName;
                    if (!LegacyCategoryMapping.TryGetValue(legacy.Name, out targetName!))
                    {
                        // If no explicit mapping, default to "Office Equipment"
                        targetName = "Office Equipment";
                        logger?.LogWarning(
                            "No mapping found for legacy category '{LegacyName}'. Defaulting to '{Target}'.",
                            legacy.Name, targetName);
                    }

                    var targetCategory = allCategories.FirstOrDefault(c => c.Name == targetName && !c.IsDeleted);
                    if (targetCategory == null)
                    {
                        logger?.LogWarning(
                            "Target category '{Target}' not found. Skipping migration for '{Legacy}'.",
                            targetName, legacy.Name);
                        continue;
                    }

                    // Reassign all assets from old → new
                    var assetsToMigrate = await context.Assets
                        .IgnoreQueryFilters()
                        .Where(a => a.CategoryId == legacy.Id)
                        .ToListAsync();

                    if (assetsToMigrate.Count > 0)
                    {
                        foreach (var asset in assetsToMigrate)
                        {
                            asset.CategoryId = targetCategory.Id;
                        }
                        logger?.LogInformation(
                            "Reassigned {Count} assets from '{From}' → '{To}'.",
                            assetsToMigrate.Count, legacy.Name, targetCategory.Name);
                    }

                    // Soft-delete the legacy category
                    legacy.IsDeleted = true;
                    logger?.LogInformation("Soft-deleted legacy category '{Name}' (ID {Id}).", legacy.Name, legacy.Id);
                }

                await context.SaveChangesAsync();
                logger?.LogInformation("Legacy category migration complete.");
            }
            else
            {
                logger?.LogInformation("No legacy categories found. Nothing to migrate.");
            }

            // ── Step 3: Seed standard divisions ──
            var allDivisions = await context.Divisions
                .IgnoreQueryFilters()
                .ToListAsync();

            var existingDivNames = allDivisions.Select(d => d.Name).ToList();

            var divsToAdd = defaultDivisions
                .Where(dd => !existingDivNames.Contains(dd))
                .Select(dd => new Division
                {
                    Name = dd,
                    Description = "Standard Division"
                })
                .ToList();

            if (divsToAdd.Count > 0)
            {
                context.Divisions.AddRange(divsToAdd);
                await context.SaveChangesAsync();
                logger?.LogInformation("Seeded {Count} default divisions.", divsToAdd.Count);

                allDivisions = await context.Divisions
                    .IgnoreQueryFilters()
                    .ToListAsync();
            }

            // ── Step 4: Reassign from legacy divisions to Admin and soft-delete ──
            var standardDivNames = defaultDivisions.ToHashSet();
            var legacyDivisions = allDivisions
                .Where(d => !d.IsDeleted && !standardDivNames.Contains(d.Name))
                .ToList();

            if (legacyDivisions.Count > 0)
            {
                var adminDivision = allDivisions.FirstOrDefault(d => d.Name == "Admin" && !d.IsDeleted);
                
                foreach (var legacy in legacyDivisions)
                {
                    if (adminDivision != null)
                    {
                        // Reassign users
                        var usersToMigrate = await context.Users
                            .IgnoreQueryFilters()
                            .Where(u => u.DivisionId == legacy.Id)
                            .ToListAsync();

                        foreach (var user in usersToMigrate)
                        {
                            user.DivisionId = adminDivision.Id;
                        }

                        // Reassign assets
                        var assetsToMigrate = await context.Assets
                            .IgnoreQueryFilters()
                            .Where(a => a.DivisionId == legacy.Id)
                            .ToListAsync();
                        
                        foreach (var asset in assetsToMigrate)
                        {
                            asset.DivisionId = adminDivision.Id;
                        }
                    }

                    legacy.IsDeleted = true;
                    logger?.LogInformation("Soft-deleted legacy division '{Name}' (ID {Id}).", legacy.Name, legacy.Id);
                }

                await context.SaveChangesAsync();
                logger?.LogInformation("Legacy division cleanup complete.");
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "An error occurred while seeding the database.");
        }
    }
}
