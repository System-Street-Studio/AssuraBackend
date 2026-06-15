// src/Assura.API/Program.cs
using Assura.API.Middleware;
using Assura.Application;
using Assura.Infrastructure;
using DotNetEnv;
using Microsoft.OpenApi.Models;

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

app.UseStaticFiles();
app.MapControllers();
app.Run();
