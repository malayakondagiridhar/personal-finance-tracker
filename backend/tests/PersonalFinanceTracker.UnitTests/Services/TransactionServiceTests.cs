using PersonalFinanceTracker.Application.Contracts.Transactions;
using PersonalFinanceTracker.Application.Exceptions;
using PersonalFinanceTracker.Infrastructure.Services;
using PersonalFinanceTracker.UnitTests.TestSupport;

namespace PersonalFinanceTracker.UnitTests.Services;

public sealed class TransactionServiceTests
{
    [Fact]
    public async Task GetAsync_InvalidDateRange_ShouldThrowValidationException()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var service = new TransactionService(dbContext);

        var query = new TransactionQuery(
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(-1),
            null,
            null,
            1,
            20,
            "transactionDateUtc",
            "desc");

        await Assert.ThrowsAsync<ValidationException>(() => service.GetAsync(query));
    }
}
