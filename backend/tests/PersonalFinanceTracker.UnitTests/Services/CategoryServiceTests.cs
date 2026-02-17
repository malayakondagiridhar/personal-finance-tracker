using PersonalFinanceTracker.Application.Contracts.Categories;
using PersonalFinanceTracker.Application.Exceptions;
using PersonalFinanceTracker.Domain.Entities;
using PersonalFinanceTracker.Infrastructure.Services;
using PersonalFinanceTracker.UnitTests.TestSupport;

namespace PersonalFinanceTracker.UnitTests.Services;

public sealed class CategoryServiceTests
{
    [Fact]
    public async Task CreateAsync_DuplicateNameForSameUser_ShouldThrowConflictException()
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

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(userId, new CreateCategoryRequest("food", null, false)));
    }
}


