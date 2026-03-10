// src/Assura.API/Program.cs
using Assura.API.Middleware;
using Assura.Application;
using Assura.Infrastructure;
using DotNetEnv;

// .env ගොනුවේ ඇති දත්ත පද්ධතියට ලබා ගැනීම
Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplication();

// .env ගොනුවේ ඇති MySQL සහ JWT දත්ත පද්ධතියට සම්බන්ධ කිරීම
builder.Configuration["ConnectionStrings:DefaultConnection"] = 
    $"Server={Env.GetString("DB_SERVER")};Port={Env.GetString("DB_PORT")};Database={Env.GetString("DB_NAME")};Uid={Env.GetString("DB_USER")};Pwd={Env.GetString("DB_PASSWORD")};";

builder.Configuration["Jwt:Key"] = Env.GetString("JWT_SECRET_KEY");
builder.Configuration["Jwt:Issuer"] = Env.GetString("JWT_ISSUER");
builder.Configuration["Jwt:Audience"] = Env.GetString("JWT_AUDIENCE");

// Infrastructure සේවාවන් එක් කිරීම (Database ඇතුළුව)
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        // .env හි ඇති ALLOWED_ORIGINS භාවිතා කිරීම
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

// දෝෂ හැසිරවීමේ Middleware එක
app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors("DefaultPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();