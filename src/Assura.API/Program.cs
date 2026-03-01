using System.Reflection;
using Assura.API.Middleware;
using Assura.Application;
using Assura.Infrastructure;
using DotNetEnv;
using Microsoft.OpenApi.Models;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Assura API",
        Description = "API for the Assura asset management system. Provides endpoints for managing divisions, assets, users, suppliers, purchasing orders, and more.",
        Contact = new OpenApiContact
        {
            Name = "System Street Studio"
        }
    });

    // Include XML comments from the API project
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    // Include XML comments from the Application project for DTOs
    var applicationXmlFilename = "Assura.Application.xml";
    var applicationXmlPath = Path.Combine(AppContext.BaseDirectory, applicationXmlFilename);
    if (File.Exists(applicationXmlPath))
    {
        options.IncludeXmlComments(applicationXmlPath);
    }

    // Add JWT Bearer authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Assura API V1");
        options.DocumentTitle = "Assura API Documentation";
    });
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors("DefaultPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();

