using Microsoft.Extensions.DependencyInjection;
using PersonalFinanceTracker.Domain.Entities;
using PersonalFinanceTracker.Infrastructure.Persistence;

namespace PersonalFinanceTracker.IntegrationTests.TestHost;

public static class TestDataSeeder
{
    public static async Task SeedUserAsync(PersonalFinanceApiFactory factory, Guid userId)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var userExists = dbContext.Users.Any(x => x.Id == userId);
        if (userExists)
        {
            return;
        }

        dbContext.Users.Add(new User
        {
            Id = userId,
            ExternalAuthId = $"test-user-{userId:N}",
            Email = $"test-{userId:N}@example.com",
            FullName = "Test User",
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }
}
