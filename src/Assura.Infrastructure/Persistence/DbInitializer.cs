using Assura.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Assura.Infrastructure.Persistence;

/// <summary>
/// Seeds the database with default reference data (categories, etc.) on application startup.
/// Only inserts rows that do not already exist (matched by Name), so it is safe to run repeatedly.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// The standard asset categories used across the organisation.
    /// </summary>
    private static readonly (string Name, string Description)[] DefaultCategories =
    {
        ("Building",                 "Land, buildings, and permanent structures"),
        ("Computer & Peripherals",   "Desktops, laptops, monitors, keyboards, mice, and related peripherals"),
        ("Satellite Antenna",        "Satellite dishes, antennas, and related communication equipment"),
        ("Lab Equipment",            "Scientific and laboratory instruments and apparatus"),
        ("Office Equipment",         "Printers, scanners, photocopiers, projectors, and general office machinery"),
        ("Furniture & Fittings",     "Desks, chairs, cabinets, shelving, and interior fittings"),
        ("Motor Vehicles",           "Cars, vans, trucks, motorcycles, and other motor vehicles"),
        ("Library Books",            "Books, journals, periodicals, and reference materials"),
    };

    /// <summary>
    /// Ensures all default categories exist in the database.
    /// Call this from Program.cs after building the application host.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetService<ILogger<AppDbContext>>();

        try
        {
            // Ensure the database schema is up to date
            await context.Database.MigrateAsync();

            // Seed categories
            var existingNames = await context.Categories
                .IgnoreQueryFilters()
                .Select(c => c.Name)
                .ToListAsync();

            var toAdd = DefaultCategories
                .Where(dc => !existingNames.Contains(dc.Name))
                .Select(dc => new Category
                {
                    Name = dc.Name,
                    Description = dc.Description,
                })
                .ToList();

            if (toAdd.Count > 0)
            {
                context.Categories.AddRange(toAdd);
                await context.SaveChangesAsync();
                logger?.LogInformation("Seeded {Count} default categories.", toAdd.Count);
            }
            else
            {
                logger?.LogInformation("All default categories already exist. No seeding needed.");
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "An error occurred while seeding the database.");
        }
    }
}
