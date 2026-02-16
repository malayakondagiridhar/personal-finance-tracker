using PersonalFinanceTracker.Application.Contracts.Categories;
using PersonalFinanceTracker.Domain.Entities;
using PersonalFinanceTracker.Infrastructure.Services;
using PersonalFinanceTracker.UnitTests.TestSupport;

namespace PersonalFinanceTracker.UnitTests.Services;

public sealed class CategoryServiceTests
{
    [Fact]
    public async Task CreateAsync_DuplicateNameForSameUser_ShouldThrowInvalidOperationException()
    {
        await using var dbContext = TestDbContextFactory.Create();

        var userId = Guid.NewGuid();
        dbContext.Users.Add(new User
        {
            Id = userId,
            ExternalAuthId = $"user-{userId:N}",
            Email = $"{userId:N}@example.com",
            FullName = "Test User"
        });

        dbContext.Categories.Add(new Category
        {
            UserId = userId,
            Name = "Food",
            IsDefault = false
        });

        await dbContext.SaveChangesAsync();

        var service = new CategoryService(dbContext);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateCategoryRequest(userId, "food", null, false)));
    }
}
