using PersonalFinanceTracker.Application.Contracts.Budgets;
using PersonalFinanceTracker.Infrastructure.Services;
using PersonalFinanceTracker.UnitTests.TestSupport;

namespace PersonalFinanceTracker.UnitTests.Services;

public sealed class BudgetServiceTests
{
    [Fact]
    public async Task CreateAsync_InvalidMonth_ShouldThrowArgumentException()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var service = new BudgetService(dbContext);

        var request = new CreateBudgetRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            2026,
            13,
            1000m);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(request));
    }
}
