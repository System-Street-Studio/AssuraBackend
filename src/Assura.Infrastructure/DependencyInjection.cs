// src/Assura.Infrastructure/DependencyInjection.cs
using Assura.Application.Common.Interfaces;
using Assura.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Assura.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            var baseConn = configuration.GetConnectionString("DefaultConnection");

            // Add keep-alive and connection lifetime params to handle remote-hosted DB restrictions
            var connectionString = baseConn!.TrimEnd(';')
                + ";Connection Timeout=60;Default Command Timeout=60;Keepalive=60;"
                + "Connection Lifetime=300;Pooling=true;Min Pool Size=1;Max Pool Size=10;";

            // Hardcoded MySQL 8.0 — avoids AutoDetect opening an extra TCP connection per request
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));
            options.UseMySql(connectionString, serverVersion, b =>
            {
                b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                // Automatically retry transient failures (dropped connections, timeouts)
                b.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            });
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        return services;
    }
}