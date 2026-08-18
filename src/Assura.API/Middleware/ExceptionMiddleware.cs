using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Assura.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception has occurred.");
            await LogExceptionToAuditAsync(httpContext, ex);
            await HandleExceptionAsync(httpContext, ex, _env);
        }
    }

    private static async Task LogExceptionToAuditAsync(HttpContext context, Exception exception)
    {
        try
        {
            var dbContext = context.RequestServices
                .GetService(typeof(Assura.Application.Common.Interfaces.IApplicationDbContext))
                as Assura.Application.Common.Interfaces.IApplicationDbContext;

            if (dbContext == null) return;

            var auditLog = new Assura.Domain.Entities.AuditLog
            {
                EntityName = "System",
                EntityId   = context.Request.Path.ToString(),
                Action     = "Error",
                OldValues  = exception.GetType().Name,
                NewValues  = exception.Message.Length > 500
                               ? exception.Message[..500]
                               : exception.Message,
                IpAddress  = context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                CreatedAt  = DateTime.UtcNow,
                CreatedBy  = context.User?.Identity?.Name ?? "System",
                Version    = 1
            };

            dbContext.AuditLogs.Add(auditLog);
            await dbContext.SaveChangesAsync(CancellationToken.None);
        }
        catch
        {
            // Audit logging failure should never mask the original exception
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception, IWebHostEnvironment env)
    {
        context.Response.ContentType = "application/json";

        var statusCode = HttpStatusCode.InternalServerError;
        var message = "Internal Server Error from the custom middleware.";

        if (exception is Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
        {
            statusCode = HttpStatusCode.Conflict;
            message = "The record you attempted to edit was modified by another user. The edit operation was canceled. Please reload the data and try again.";
        }
        else if (exception is FluentValidation.ValidationException validationException)
        {
            statusCode = HttpStatusCode.BadRequest;
            message = string.Join(" ", validationException.Errors.Select(e => e.ErrorMessage));
        }

        context.Response.StatusCode = (int)statusCode;

        var result = JsonSerializer.Serialize(new
        {
            StatusCode = context.Response.StatusCode,
            Message    = message,
            Detail     = env.IsDevelopment()
                ? $"{exception.Message}{(exception.InnerException != null ? " | Inner: " + exception.InnerException.Message : "")}"
                : "An unexpected error occurred.",
            StackTrace = env.IsDevelopment() ? exception.StackTrace : null
        });

        return context.Response.WriteAsync(result);
    }
}
