namespace PersonalFinanceTracker.Api.Middleware;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }

    public static IApplicationBuilder UseUserProfileSync(this IApplicationBuilder app)
    {
        return app.UseMiddleware<UserProfileSyncMiddleware>();
    }
}
