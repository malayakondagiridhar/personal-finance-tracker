using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Application.Exceptions;

namespace PersonalFinanceTracker.Api.Middleware;

public sealed class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            var (statusCode, title) = MapException(exception);

            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", context.TraceIdentifier);
            }
            else
            {
                logger.LogWarning(exception, "Handled exception. TraceId: {TraceId}", context.TraceIdentifier);
            }

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                title,
                status = statusCode,
                detail = exception.Message,
                traceId = context.TraceIdentifier
            };

            var payload = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(payload);
        }
    }

    private static (int StatusCode, string Title) MapException(Exception exception)
    {
        return exception switch
        {
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            ConflictException => (StatusCodes.Status409Conflict, "Business rule conflict"),
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Concurrency conflict"),
            ValidationException => (StatusCodes.Status400BadRequest, "Invalid request"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request"),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected server error")
        };
    }
}
