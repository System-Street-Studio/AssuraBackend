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

static string? GetFirstEnvValue(params string[] keys)
{
    foreach (var key in keys)
    {
        var value = Env.GetString(key)?.Trim();
        if (!string.IsNullOrWhiteSpace(value)) return value;
    }
    return null;
}

builder.Services.AddApplication();

// Backs the short-lived dashboard/lookup caches; the database is remote, so avoiding
// repeat round-trips matters more than absolute freshness on read-only overviews.
builder.Services.AddMemoryCache();

var dbConnectionStringFromEnv = GetFirstEnvValue("DB_CONNECTION_STRING", "MYSQL_CONNECTION_STRING");
var dbServer = GetFirstEnvValue("DB_SERVER", "DB_HOST");
var dbPort = GetFirstEnvValue("DB_PORT") ?? "3306";
var dbName = GetFirstEnvValue("DB_NAME");
var dbUser = GetFirstEnvValue("DB_USER", "DB_USERNAME");
var dbPassword = GetFirstEnvValue("DB_PASSWORD", "DB_PASS");
var dbSslMode = GetFirstEnvValue("DB_SSL_MODE");

if (!string.IsNullOrWhiteSpace(dbServer) && !string.IsNullOrWhiteSpace(dbName) && !string.IsNullOrWhiteSpace(dbUser) && !string.IsNullOrWhiteSpace(dbPassword))
{
    var dbConnectionString = dbConnectionStringFromEnv ?? $"Server={dbServer};Port={dbPort};Database={dbName};Uid={dbUser};Pwd={dbPassword};";
    if (!string.IsNullOrWhiteSpace(dbSslMode)) dbConnectionString += $"SslMode={dbSslMode};";
    builder.Configuration["ConnectionStrings:DefaultConnection"] = dbConnectionString;
}

var envConn = Env.GetString("DB_CONNECTION_STRING") ?? Env.GetString("MYSQL_CONNECTION_STRING");
if (!string.IsNullOrWhiteSpace(envConn)) builder.Configuration["ConnectionStrings:DefaultConnection"] = envConn;

builder.Configuration["Jwt:Key"] = Env.GetString("JWT_SECRET_KEY") ?? builder.Configuration["Jwt:Key"];
builder.Configuration["Jwt:Issuer"] = Env.GetString("JWT_ISSUER") ?? builder.Configuration["Jwt:Issuer"];
builder.Configuration["Jwt:Audience"] = Env.GetString("JWT_AUDIENCE") ?? builder.Configuration["Jwt:Audience"];
builder.Configuration["Jwt:ExpiryMinutes"] = Env.GetString("JWT_EXPIRY_MINUTES", builder.Configuration["Jwt:ExpiryMinutes"] ?? "60");

builder.Configuration["Smtp:Host"] = GetFirstEnvValue("SMTP_HOST") ?? builder.Configuration["Smtp:Host"];
builder.Configuration["Smtp:Port"] = GetFirstEnvValue("SMTP_PORT") ?? builder.Configuration["Smtp:Port"];
builder.Configuration["Smtp:Username"] = GetFirstEnvValue("SMTP_USER", "SMTP_USERNAME") ?? builder.Configuration["Smtp:Username"];
builder.Configuration["Smtp:Password"] = GetFirstEnvValue("SMTP_PASSWORD") ?? builder.Configuration["Smtp:Password"];
// FromEmail must be a real email address (used to build a System.Net.Mail.MailAddress);
// FromName is just the display name. These used to be conflated - falling back
// FromEmail to SMTP_FROM_NAME let a deployment that only set SMTP_FROM_NAME pass a
// non-address string into MailAddress, throwing and silently killing all outbound
// password-reset email.
builder.Configuration["Smtp:FromEmail"] = GetFirstEnvValue("SMTP_FROM_EMAIL") ?? builder.Configuration["Smtp:FromEmail"];
builder.Configuration["Smtp:FromName"] = GetFirstEnvValue("SMTP_FROM_NAME") ?? builder.Configuration["Smtp:FromName"];

builder.Configuration["App:FrontendBaseUrl"] = GetFirstEnvValue("FRONTEND_BASE_URL") ?? builder.Configuration["App:FrontendBaseUrl"];

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
