using System.Net;
using System.Text.Json;

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
            logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", context.TraceIdentifier);

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                title = "Unexpected server error",
                status = context.Response.StatusCode,
                traceId = context.TraceIdentifier
            };

            var payload = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(payload);
        }
    }
}
