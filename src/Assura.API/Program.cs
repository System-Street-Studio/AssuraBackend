using Assura.API.Middleware;
using Assura.Application;
using Assura.Infrastructure;
using DotNetEnv;
using Microsoft.OpenApi.Models;

// Load .env file
Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
       /* var allowedOrigins = builder.Configuration["ALLOWED_ORIGINS"]?.Split(',') ?? new[] { "http://localhost:4200" };*/
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FAMS API",
        Version = "v1",
        Description = "Fixed Asset Management System API for Employees (No Auth)"
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
        c.RoutePrefix = string.Empty; // Swagger at root
    });
    app.UseDeveloperExceptionPage(); // Shows inner exception
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors("DefaultPolicy");

app.MapControllers(); // Controllers accessible without JWT/Authorize

app.Run();