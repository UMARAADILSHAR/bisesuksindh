using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace BiseHyderabad.Web.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception processing request {Method} {Path}", context.Request.Method, context.Request.Path);

            if (context.Response.HasStarted)
            {
                _logger.LogWarning("The response has already started, exception handler cannot write problem details.");
                throw;
            }

            if (context.Request.Path.StartsWithSegments("/api"))
            {
                await HandleApiExceptionAsync(context, ex);
            }
            else
            {
                // Let Blazor error boundaries or exception page handle browser rendering
                throw;
            }
        }
    }

    private static async Task HandleApiExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var problemDetails = new ProblemDetails
        {
            Status = (int)HttpStatusCode.InternalServerError,
            Title = "An internal server error occurred while processing your request.",
            Detail = "An unexpected error occurred. Please contact BISE Hyderabad IT Support.",
            Instance = context.Request.Path
        };

        var json = JsonSerializer.Serialize(problemDetails);
        await context.Response.WriteAsync(json);
    }
}

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
