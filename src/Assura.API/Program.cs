// src/Assura.API/Program.cs
using Assura.API.Middleware;
using Assura.Application;
using Assura.Infrastructure;
using DotNetEnv;

// Load environment variables from .env file
Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplication();

// Map MySQL and JWT data from .env file to connection strings
var connStr = $"Server={Env.GetString("DB_SERVER")};Port={Env.GetString("DB_PORT")};Database={Env.GetString("DB_NAME")};Uid={Env.GetString("DB_USER")};Pwd={Env.GetString("DB_PASSWORD")};";
Console.WriteLine($"[DEBUG] Connection String: {connStr}");
builder.Configuration["ConnectionStrings:DefaultConnection"] = connStr;

builder.Configuration["Jwt:Key"] = Env.GetString("JWT_SECRET_KEY");
builder.Configuration["Jwt:Issuer"] = Env.GetString("JWT_ISSUER");
builder.Configuration["Jwt:Audience"] = Env.GetString("JWT_AUDIENCE");

// Add Infrastructure services (including Database)
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();

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

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Exception handling middleware
app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors("DefaultPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/apply-sql", async (Assura.Infrastructure.Persistence.AppDbContext db) => {
    var sql = System.IO.File.ReadAllText(@"C:\temp\my_migration.sql");
    using var transaction = await db.Database.BeginTransactionAsync();
    try {
        await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(db.Database, sql);
        await transaction.CommitAsync();
        return "Applied";
    } catch (Exception ex) {
        return $"Error: {ex.Message}";
    }
});

app.Run();