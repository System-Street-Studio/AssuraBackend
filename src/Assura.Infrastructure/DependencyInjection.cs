// src/Assura.Infrastructure/DependencyInjection.cs
using Assura.Application.Common.Interfaces;
using Assura.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Assura.Infrastructure.Identity;
using Assura.Infrastructure.Services;

namespace Assura.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            var baseConn = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;

            // Add keep-alive and connection lifetime params to handle remote-hosted DB restrictions
            var connectionString = baseConn.TrimEnd(';')
                + ";Connection Timeout=60;Default Command Timeout=60;Keepalive=60;"
                + "Connection Lifetime=300;Pooling=true;Min Pool Size=1;Max Pool Size=10;";

            // Use configured server version if present, otherwise fall back to default
            var serverVersionStr = configuration["Database:ServerVersion"] ?? "10.11.15-mariadb";
            options.UseMySql(connectionString, ServerVersion.Parse(serverVersionStr), b =>
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
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddHttpContextAccessor();

        // Custom Auth Services from feature/auth
        services.AddScoped<IIdentifyServices, IdentityService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        var jwtSettings = configuration.GetSection("Jwt");
        var secretKey = jwtSettings.GetValue<string>("Key") ?? "YourDevelopmentSecretKeyChangeInProduction";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.GetValue<string>("Issuer"),
                ValidAudience = jwtSettings.GetValue<string>("Audience"),
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            };
        });

        services.AddAuthorization();

        return services;
    }
}