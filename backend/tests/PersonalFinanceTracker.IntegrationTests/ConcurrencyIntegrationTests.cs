using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using PersonalFinanceTracker.Api.Middleware;

namespace PersonalFinanceTracker.IntegrationTests;

public sealed class ConcurrencyIntegrationTests
{
    [Fact]
    public async Task GlobalExceptionMiddleware_DbUpdateConcurrencyException_ShouldReturn409Contract()
    {
        RequestDelegate next = _ => throw new DbUpdateConcurrencyException("Simulated optimistic concurrency conflict.");
        var middleware = new GlobalExceptionMiddleware(next, new NullLogger<GlobalExceptionMiddleware>());

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);

        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var payload = await reader.ReadToEndAsync();

        var json = JsonSerializer.Deserialize<JsonElement>(payload);
        Assert.Equal(409, json.GetProperty("status").GetInt32());
        Assert.Equal("Concurrency conflict", json.GetProperty("title").GetString());
        Assert.True(json.TryGetProperty("traceId", out _));
    }
}
