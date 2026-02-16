using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Application.Abstractions.Services;
using PersonalFinanceTracker.Application.Contracts.Budgets;
using PersonalFinanceTracker.Domain.Entities;
using PersonalFinanceTracker.Domain.Enums;
using PersonalFinanceTracker.Infrastructure.Persistence;

namespace PersonalFinanceTracker.Infrastructure.Services;

public sealed class BudgetService(AppDbContext dbContext) : IBudgetService
{
    public async Task<BudgetDto> CreateAsync(CreateBudgetRequest request, CancellationToken cancellationToken = default)
    {
        EnsurePeriodIsValid(request.Year, request.Month);
        await EnsureCategoryBelongsToUserAsync(request.CategoryId, request.UserId, cancellationToken);

        var existingBudget = await dbContext.Budgets
            .AsNoTracking()
            .AnyAsync(
                x => x.UserId == request.UserId
                     && x.CategoryId == request.CategoryId
                     && x.Year == request.Year
                     && x.Month == request.Month,
                cancellationToken);

        if (existingBudget)
        {
            throw new InvalidOperationException("Budget already exists for this category and month.");
        }

        var budget = new Budget
        {
            UserId = request.UserId,
            CategoryId = request.CategoryId,
            Year = request.Year,
            Month = request.Month,
            LimitAmount = request.LimitAmount,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        dbContext.Budgets.Add(budget);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new BudgetDto(
            budget.Id,
            budget.UserId,
            budget.CategoryId,
            budget.Year,
            budget.Month,
            budget.LimitAmount);
    }

    public async Task<IReadOnlyList<BudgetStatusDto>> GetMonthlyStatusAsync(Guid userId, int year, int month, CancellationToken cancellationToken = default)
    {
        EnsurePeriodIsValid(year, month);

        var budgets = await dbContext.Budgets
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Year == year && x.Month == month)
            .Join(
                dbContext.Categories.AsNoTracking(),
                budget => budget.CategoryId,
                category => category.Id,
                (budget, category) => new
                {
                    budget.Id,
                    budget.CategoryId,
                    CategoryName = category.Name,
                    budget.LimitAmount
                })
            .ToListAsync(cancellationToken);

        if (budgets.Count == 0)
        {
            return [];
        }

        var periodStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEnd = periodStart.AddMonths(1);

        var spentByCategory = await dbContext.Transactions
            .AsNoTracking()
            .Where(x => x.UserId == userId
                        && x.Type == TransactionType.Expense
                        && x.TransactionDateUtc >= periodStart
                        && x.TransactionDateUtc < periodEnd)
            .GroupBy(x => x.CategoryId)
            .Select(g => new
            {
                CategoryId = g.Key,
                SpentAmount = g.Sum(x => x.Amount)
            })
            .ToDictionaryAsync(x => x.CategoryId, x => x.SpentAmount, cancellationToken);

        return budgets
            .Select(x =>
            {
                var spentAmount = spentByCategory.GetValueOrDefault(x.CategoryId, 0m);
                return new BudgetStatusDto(
                    x.Id,
                    x.CategoryId,
                    x.CategoryName,
                    x.LimitAmount,
                    spentAmount,
                    x.LimitAmount - spentAmount);
            })
            .ToList();
    }

    private async Task EnsureCategoryBelongsToUserAsync(Guid categoryId, Guid userId, CancellationToken cancellationToken)
    {
        var categoryExists = await dbContext.Categories
            .AsNoTracking()
            .AnyAsync(x => x.Id == categoryId && x.UserId == userId, cancellationToken);

        if (!categoryExists)
        {
            throw new InvalidOperationException("Category was not found for this user.");
        }
    }

    private static void EnsurePeriodIsValid(int year, int month)
    {
        if (year is < 2000 or > 2100)
        {
            throw new ArgumentException("year must be between 2000 and 2100.");
        }

        if (month is < 1 or > 12)
        {
            throw new ArgumentException("month must be between 1 and 12.");
        }
    }
}
