// src/Assura.API/Program.cs
using Assura.API.Middleware;
using Assura.Application;
using Assura.Infrastructure;
using Assura.Infrastructure.Persistence;
using DotNetEnv;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Prometheus;
using Serilog;
using Serilog.Formatting.Compact;

Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .WriteTo.Console(new CompactJsonFormatter()));

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

// Liveness ("live") intentionally has no dependency checks — a brief DB blip shouldn't cause
// every pod to be killed and restarted at once. Only readiness ("ready") checks the DB, so
// k8s pulls a pod out of Service traffic during an outage without restarting it.
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("mysql", tags: new[] { "ready" });

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
builder.Configuration["Smtp:FromEmail"] = GetFirstEnvValue("SMTP_FROM_EMAIL", "SMTP_FROM_NAME") ?? builder.Configuration["Smtp:FromEmail"];

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
        var allowedOrigins = builder.Configuration["ALLOWED_ORIGINS"]?.Split(',') ?? new[] { "http://localhost:4200" };
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
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

// Three ways this process can be started, controlled by env vars set differently for local dev
// vs. Kubernetes:
//  - Default (local dev, docker-compose): seed/migrate runs on every boot, same as always.
//  - MIGRATE_ONLY=true: run the seed/migrate step, then exit without starting Kestrel. This is
//    what the Helm pre-upgrade hook Job runs — exactly one runner applies pending EF Core
//    migrations before the Deployment rolls, instead of every replica racing to migrate at once.
//  - SKIP_AUTO_MIGRATE=true: skip the seed/migrate step entirely and go straight to Kestrel.
//    Set on the Deployment's regular pods in Kubernetes, since the migration Job already ran it.
var migrateOnly = args.Contains("--migrate-only") || Env.GetBool("MIGRATE_ONLY", false);
var skipAutoMigrate = Env.GetBool("SKIP_AUTO_MIGRATE", false);

if (migrateOnly || !skipAutoMigrate)
{
    // Seed default categories (Building, Computer & Peripherals, etc.) into the database
    await DbInitializer.SeedAsync(app.Services);
}

if (migrateOnly)
{
    return;
}

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

app.UseSerilogRequestLogging();
app.UseHttpMetrics();

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("DefaultPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.MapControllers();
app.MapMetrics();
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") });
app.Run();
