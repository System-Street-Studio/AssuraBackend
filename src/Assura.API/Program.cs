// src/Assura.API/Program.cs
using Assura.API.Middleware;
using Assura.Application;
using Assura.Infrastructure;
using Assura.Infrastructure.Persistence;
using DotNetEnv;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using System.Threading.RateLimiting;

Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

static string? GetFirstEnvValue(IConfiguration? config, params string[] keys)
{
    foreach (var key in keys)
    {
        var sysVal = Environment.GetEnvironmentVariable(key)?.Trim();
        if (!string.IsNullOrWhiteSpace(sysVal)) return sysVal;

        if (config != null)
        {
            var cfgVal = config[key]?.Trim();
            if (!string.IsNullOrWhiteSpace(cfgVal)) return cfgVal;
        }

        try
        {
            var envVal = Env.GetString(key)?.Trim();
            if (!string.IsNullOrWhiteSpace(envVal)) return envVal;
        }
        catch { }
    }
    return null;
}

static string? ParseMySqlConnectionString(string? raw)
{
    if (string.IsNullOrWhiteSpace(raw)) return null;
    raw = raw.Trim();
    if (raw.StartsWith("mysql://", StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            var uri = new Uri(raw);
            var userInfo = uri.UserInfo.Split(':');
            var user = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "";
            var pass = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
            var host = uri.Host;
            var port = uri.Port > 0 ? uri.Port : 3306;
            var db = uri.AbsolutePath.TrimStart('/');
            return $"Server={host};Port={port};Database={db};Uid={user};Pwd={pass};";
        }
        catch
        {
            return raw;
        }
    }
    return raw;
}

builder.Services.AddApplication();

// Backs the short-lived dashboard/lookup caches; the database is remote, so avoiding
// repeat round-trips matters more than absolute freshness on read-only overviews.
builder.Services.AddMemoryCache();

var directConnStr = ParseMySqlConnectionString(
    GetFirstEnvValue(builder.Configuration,
        "DB_CONNECTION_STRING",
        "MYSQL_CONNECTION_STRING",
        "DATABASE_URL",
        "MYSQL_URL",
        "MYSQL_PUBLIC_URL",
        "MYSQL_PRIVATE_URL",
        "ConnectionStrings:DefaultConnection",
        "ConnectionStrings__DefaultConnection"));

var dbServer = GetFirstEnvValue(builder.Configuration, "DB_SERVER", "DB_HOST", "MYSQLHOST", "MYSQL_HOST");
var dbPort = GetFirstEnvValue(builder.Configuration, "DB_PORT", "MYSQLPORT", "MYSQL_PORT") ?? "3306";
var dbName = GetFirstEnvValue(builder.Configuration, "DB_NAME", "MYSQLDATABASE", "MYSQL_DATABASE");
var dbUser = GetFirstEnvValue(builder.Configuration, "DB_USER", "DB_USERNAME", "MYSQLUSER", "MYSQL_USER");
var dbPassword = GetFirstEnvValue(builder.Configuration, "DB_PASSWORD", "DB_PASS", "MYSQLPASSWORD", "MYSQL_PASSWORD");
var dbSslMode = GetFirstEnvValue(builder.Configuration, "DB_SSL_MODE", "MYSQL_SSL_MODE");

if (!string.IsNullOrWhiteSpace(directConnStr))
{
    builder.Configuration["ConnectionStrings:DefaultConnection"] = directConnStr;
}
else if (!string.IsNullOrWhiteSpace(dbServer) && !string.IsNullOrWhiteSpace(dbName) && !string.IsNullOrWhiteSpace(dbUser) && !string.IsNullOrWhiteSpace(dbPassword))
{
    var dbConnectionString = $"Server={dbServer};Port={dbPort};Database={dbName};Uid={dbUser};Pwd={dbPassword};";
    if (!string.IsNullOrWhiteSpace(dbSslMode)) dbConnectionString += $"SslMode={dbSslMode};";
    builder.Configuration["ConnectionStrings:DefaultConnection"] = dbConnectionString;
}

builder.Configuration["Jwt:Key"] = GetFirstEnvValue(builder.Configuration, "JWT_SECRET_KEY", "Jwt:Key") ?? builder.Configuration["Jwt:Key"];
builder.Configuration["Jwt:Issuer"] = GetFirstEnvValue(builder.Configuration, "JWT_ISSUER", "Jwt:Issuer") ?? builder.Configuration["Jwt:Issuer"];
builder.Configuration["Jwt:Audience"] = GetFirstEnvValue(builder.Configuration, "JWT_AUDIENCE", "Jwt:Audience") ?? builder.Configuration["Jwt:Audience"];
builder.Configuration["Jwt:ExpiryMinutes"] = GetFirstEnvValue(builder.Configuration, "JWT_EXPIRY_MINUTES", "Jwt:ExpiryMinutes") ?? builder.Configuration["Jwt:ExpiryMinutes"] ?? "60";

builder.Configuration["Smtp:Host"] = GetFirstEnvValue(builder.Configuration, "SMTP_HOST", "Smtp:Host") ?? builder.Configuration["Smtp:Host"];
builder.Configuration["Smtp:Port"] = GetFirstEnvValue(builder.Configuration, "SMTP_PORT", "Smtp:Port") ?? builder.Configuration["Smtp:Port"];
builder.Configuration["Smtp:Username"] = GetFirstEnvValue(builder.Configuration, "SMTP_USER", "SMTP_USERNAME", "Smtp:Username") ?? builder.Configuration["Smtp:Username"];
builder.Configuration["Smtp:Password"] = GetFirstEnvValue(builder.Configuration, "SMTP_PASSWORD", "Smtp:Password") ?? builder.Configuration["Smtp:Password"];
builder.Configuration["Smtp:FromEmail"] = GetFirstEnvValue(builder.Configuration, "SMTP_FROM_EMAIL", "Smtp:FromEmail") ?? builder.Configuration["Smtp:FromEmail"];
builder.Configuration["Smtp:FromName"] = GetFirstEnvValue(builder.Configuration, "SMTP_FROM_NAME", "Smtp:FromName") ?? builder.Configuration["Smtp:FromName"];

builder.Configuration["App:FrontendBaseUrl"] = GetFirstEnvValue(builder.Configuration, "FRONTEND_BASE_URL", "App:FrontendBaseUrl") ?? builder.Configuration["App:FrontendBaseUrl"];

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        var allowedOriginsEnv = builder.Configuration["ALLOWED_ORIGINS"];
        if (!string.IsNullOrWhiteSpace(allowedOriginsEnv))
        {
            var origins = allowedOriginsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            policy.WithOrigins(origins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
        else
        {
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});

// Throttles the password-reset endpoints per client IP so they can't be used to
// flood an arbitrary victim's inbox or brute-force a reset token.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("PasswordReset", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 5,
                QueueLimit = 0
            }));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FAMS API", Version = "v1", Description = "Fixed Asset Management System API" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, new string[] {} }
    });
});

var app = builder.Build();

// Seed default categories (Building, Computer & Peripherals, etc.) into the database
await DbInitializer.SeedAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FAMS API v1"));
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHttpsRedirection();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("DefaultPolicy");

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.UseStaticFiles();
app.MapControllers();
app.Run();
