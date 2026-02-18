using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Domain.Entities;
using PersonalFinanceTracker.Domain.Enums;

namespace PersonalFinanceTracker.Infrastructure.Persistence;

public static class DemoDataSeeder
{
    private static readonly Guid DemoUserId = Guid.Parse("4f4f9021-5e6a-4f8c-9b5f-c2cc8d17f1f1");
    private static readonly Guid FoodCategoryId = Guid.Parse("fb54f7a0-45df-4d7d-8ca8-5f05f3657f75");
    private static readonly Guid SalaryCategoryId = Guid.Parse("8ed6f6b5-f4f2-4b4a-84f2-cb66a5fd22e9");
    private static readonly Guid UtilitiesCategoryId = Guid.Parse("7c6ff1b7-d8e2-4d66-bb4f-77d8a4f4ac10");

    public static async Task SeedAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var existingUser = await dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == DemoUserId, cancellationToken);

        if (existingUser is null)
        {
            existingUser = new User
            {
                Id = DemoUserId,
                ExternalAuthId = "demo-user-local",
                Email = "demo.user@personal-finance.local",
                FullName = "Demo User",
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            dbContext.Users.Add(existingUser);
        }

        var categories = new[]
        {
            new Category
            {
                Id = FoodCategoryId,
                UserId = DemoUserId,
                Name = "Food",
                NormalizedName = "FOOD",
                Description = "Groceries and eating out",
                IsDefault = true,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            },
            new Category
            {
                Id = SalaryCategoryId,
                UserId = DemoUserId,
                Name = "Salary",
                NormalizedName = "SALARY",
                Description = "Primary monthly income",
                IsDefault = true,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            },
            new Category
            {
                Id = UtilitiesCategoryId,
                UserId = DemoUserId,
                Name = "Utilities",
                NormalizedName = "UTILITIES",
                Description = "Electricity, internet, and bills",
                IsDefault = false,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            }
        };

        foreach (var category in categories)
        {
            var exists = await dbContext.Categories
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Id == category.Id, cancellationToken);

            if (!exists)
            {
                dbContext.Categories.Add(category);
            }
        }

        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var transactions = new[]
        {
            new Transaction
            {
                Id = Guid.Parse("5a2d15bb-b890-4d8d-a799-3109afea4a56"),
                UserId = DemoUserId,
                CategoryId = SalaryCategoryId,
                Amount = 85000m,
                Type = TransactionType.Income,
                TransactionDateUtc = startOfMonth.AddDays(1),
                Note = "Monthly salary",
                IsDeleted = false,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            },
            new Transaction
            {
                Id = Guid.Parse("75256063-d4dd-45a5-ad74-ce64ef45e9a3"),
                UserId = DemoUserId,
                CategoryId = FoodCategoryId,
                Amount = 4200m,
                Type = TransactionType.Expense,
                TransactionDateUtc = startOfMonth.AddDays(4),
                Note = "Groceries",
                IsDeleted = false,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            },
            new Transaction
            {
                Id = Guid.Parse("57f7f421-ce1f-4c80-a41d-e36c369ca211"),
                UserId = DemoUserId,
                CategoryId = UtilitiesCategoryId,
                Amount = 2500m,
                Type = TransactionType.Expense,
                TransactionDateUtc = startOfMonth.AddDays(6),
                Note = "Electricity bill",
                IsDeleted = false,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            }
        };

        foreach (var transaction in transactions)
        {
            var exists = await dbContext.Transactions
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Id == transaction.Id, cancellationToken);

            if (!exists)
            {
                dbContext.Transactions.Add(transaction);
            }
        }

        var budgetExists = await dbContext.Budgets
            .IgnoreQueryFilters()
            .AnyAsync(x =>
                x.UserId == DemoUserId &&
                x.CategoryId == FoodCategoryId &&
                x.Year == now.Year &&
                x.Month == now.Month,
                cancellationToken);

        if (!budgetExists)
        {
            dbContext.Budgets.Add(new Budget
            {
                Id = Guid.Parse("b9e45cc7-98fc-43b8-9779-f83d60f3687d"),
                UserId = DemoUserId,
                CategoryId = FoodCategoryId,
                Year = now.Year,
                Month = now.Month,
                LimitAmount = 12000m,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
