using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Api.Auth;
using PersonalFinanceTracker.Domain.Entities;
using PersonalFinanceTracker.Infrastructure.Persistence;

namespace PersonalFinanceTracker.Api.Middleware;

public sealed class UserProfileSyncMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var principal = context.User;

        if (principal?.Identity?.IsAuthenticated == true)
        {
            var userId = principal.GetRequiredUserId();
            var externalAuthId = principal.GetExternalAuthId();
            var email = principal.FindFirstValue(ClaimTypes.Email) ?? $"user-{userId:N}@local.invalid";
            var fullName = principal.FindFirstValue(ClaimTypes.Name) ?? "Authenticated User";

            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var existingUser = await dbContext.Users
                .FirstOrDefaultAsync(x => x.ExternalAuthId == externalAuthId || x.Id == userId, context.RequestAborted);

            if (existingUser is null)
            {
                dbContext.Users.Add(new User
                {
                    Id = userId,
                    ExternalAuthId = externalAuthId,
                    Email = email,
                    FullName = fullName,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow
                });

                await dbContext.SaveChangesAsync(context.RequestAborted);
            }
            else
            {
                var hasChanges = false;

                if (!string.Equals(existingUser.ExternalAuthId, externalAuthId, StringComparison.Ordinal))
                {
                    existingUser.ExternalAuthId = externalAuthId;
                    hasChanges = true;
                }

                if (!string.Equals(existingUser.Email, email, StringComparison.OrdinalIgnoreCase))
                {
                    existingUser.Email = email;
                    hasChanges = true;
                }

                if (!string.Equals(existingUser.FullName, fullName, StringComparison.Ordinal))
                {
                    existingUser.FullName = fullName;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    existingUser.UpdatedAtUtc = DateTime.UtcNow;
                    await dbContext.SaveChangesAsync(context.RequestAborted);
                }
            }
        }

        await next(context);
    }
}
