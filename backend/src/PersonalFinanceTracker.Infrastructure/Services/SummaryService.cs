using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Application.Abstractions.Services;
using PersonalFinanceTracker.Application.Contracts.Summaries;
using PersonalFinanceTracker.Domain.Enums;
using PersonalFinanceTracker.Infrastructure.Persistence;

namespace PersonalFinanceTracker.Infrastructure.Services;

public sealed class SummaryService(AppDbContext dbContext) : ISummaryService
{
    public async Task<MonthlySummaryDto> GetMonthlySummaryAsync(Guid userId, int year, int month, CancellationToken cancellationToken = default)
    {
        var periodStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEnd = periodStart.AddMonths(1);

        var transactions = dbContext.Transactions
            .AsNoTracking()
            .Where(x => x.UserId == userId
                        && x.TransactionDateUtc >= periodStart
                        && x.TransactionDateUtc < periodEnd);

        var totalIncome = await transactions
            .Where(x => x.Type == TransactionType.Income)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var totalExpense = await transactions
            .Where(x => x.Type == TransactionType.Expense)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var categoryBreakdownRaw = await transactions
            .Where(x => x.Type == TransactionType.Expense)
            .Join(
                dbContext.Categories.AsNoTracking(),
                transaction => transaction.CategoryId,
                category => category.Id,
                (transaction, category) => new
                {
                    category.Id,
                    category.Name,
                    transaction.Amount
                })
            .GroupBy(x => new { x.Id, x.Name })
            .Select(group => new
            {
                group.Key.Id,
                group.Key.Name,
                Amount = group.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.Amount)
            .ToListAsync(cancellationToken);

        var categoryBreakdown = categoryBreakdownRaw
            .Select(x => new CategorySpendDto(x.Id, x.Name, x.Amount))
            .ToList();

        return new MonthlySummaryDto(
            userId,
            year,
            month,
            totalIncome,
            totalExpense,
            totalIncome - totalExpense,
            categoryBreakdown);
    }
}
