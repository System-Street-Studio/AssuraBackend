// src/Assura.API/Program.cs
using Assura.API.Middleware;
using Assura.Application;
using Assura.Infrastructure;
using DotNetEnv;
using Microsoft.OpenApi.Models;

<<<<<<< HEAD
=======
// Load environment variables from .env file
>>>>>>> feature/system-updates-dev
Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

static string? GetFirstEnvValue(params string[] keys)
{
    foreach (var key in keys)
    {
        using Assura.API.Middleware;
        using Assura.Application;
        using Assura.Infrastructure;
        using DotNetEnv;
        using Microsoft.OpenApi.Models;

        // Load environment variables from .env file
        Env.TraversePath().Load();

        var builder = WebApplication.CreateBuilder(args);

        static string? GetFirstEnvValue(params string[] keys)
        {
            foreach (var key in keys)
            {
                var value = Env.GetString(key)?.Trim();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return null;
        }

        // Add services
        builder.Services.AddApplication();

        // Prefer an explicit connection string from .env if present, otherwise fall back to individual vars
        var dbConnectionStringFromEnv = GetFirstEnvValue("DB_CONNECTION_STRING", "MYSQL_CONNECTION_STRING");
        var dbServer = GetFirstEnvValue("DB_SERVER", "DB_HOST");
        var dbPort = GetFirstEnvValue("DB_PORT") ?? "3306";
        var dbName = GetFirstEnvValue("DB_NAME");
        var dbUser = GetFirstEnvValue("DB_USER", "DB_USERNAME");
        var dbPassword = GetFirstEnvValue("DB_PASSWORD", "DB_PASS");
        var dbSslMode = GetFirstEnvValue("DB_SSL_MODE");

        // If someone provided the raw pieces in .env, compose them and set Configuration
        if (!string.IsNullOrWhiteSpace(dbServer) && !string.IsNullOrWhiteSpace(dbName) && !string.IsNullOrWhiteSpace(dbUser) && !string.IsNullOrWhiteSpace(dbPassword))
        {
            var dbConnectionString = dbConnectionStringFromEnv ?? $"Server={dbServer};Port={dbPort};Database={dbName};Uid={dbUser};Pwd={dbPassword};";
            if (!string.IsNullOrWhiteSpace(dbSslMode)) dbConnectionString += $"SslMode={dbSslMode};";
            builder.Configuration["ConnectionStrings:DefaultConnection"] = dbConnectionString;
        }

        // Also accept a single connection string in environment
        var envConn = Env.GetString("DB_CONNECTION_STRING") ?? Env.GetString("MYSQL_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(envConn)) builder.Configuration["ConnectionStrings:DefaultConnection"] = envConn;

        builder.Configuration["Jwt:Key"] = Env.GetString("JWT_SECRET_KEY") ?? builder.Configuration["Jwt:Key"];
        builder.Configuration["Jwt:Issuer"] = Env.GetString("JWT_ISSUER") ?? builder.Configuration["Jwt:Issuer"];
        builder.Configuration["Jwt:Audience"] = Env.GetString("JWT_AUDIENCE") ?? builder.Configuration["Jwt:Audience"];
        builder.Configuration["Jwt:ExpiryMinutes"] = Env.GetString("JWT_EXPIRY_MINUTES", builder.Configuration["Jwt:ExpiryMinutes"] ?? "60");

        // Add Infrastructure services (including Database)
        builder.Services.AddInfrastructure(builder.Configuration);

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });

        // Configure CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("DefaultPolicy", policy =>
            {
                // Use ALLOWED_ORIGINS from .env
                var allowedOrigins = builder.Configuration["ALLOWED_ORIGINS"]?.Split(',') ?? new[] { "http://localhost:4200" };
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FAMS API v1");
        // c.RoutePrefix = string.Empty; // Swagger at root
    });
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHttpsRedirection();
}

<<<<<<< HEAD
=======
// Exception handling middleware
>>>>>>> feature/system-updates-dev
app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("DefaultPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles(); // Serve uploaded receipt files from wwwroot

app.MapControllers();
app.Run();
