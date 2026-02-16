using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Infrastructure.Persistence;

namespace PersonalFinanceTracker.UnitTests.TestSupport;

public static class TestDbContextFactory
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"UnitTests_{Guid.NewGuid():N}")
            .Options;

        return new AppDbContext(options);
    }
}
