using PersonalFinanceTracker.Application.Contracts.Budgets;
using PersonalFinanceTracker.Application.Exceptions;
using PersonalFinanceTracker.Infrastructure.Services;
using PersonalFinanceTracker.UnitTests.TestSupport;

namespace PersonalFinanceTracker.UnitTests.Services;

public sealed class BudgetServiceTests
{
    [Fact]
    public async Task CreateAsync_InvalidMonth_ShouldThrowValidationException()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var service = new BudgetService(dbContext);

        var request = new CreateBudgetRequest(
            Guid.NewGuid(),
            2026,
            13,
            1000m);

        await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(Guid.NewGuid(), request));
    }
}


