using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Application.Abstractions.Services;
using PersonalFinanceTracker.Application.Contracts.Categories;
using PersonalFinanceTracker.Domain.Entities;
using PersonalFinanceTracker.Infrastructure.Persistence;

namespace PersonalFinanceTracker.Infrastructure.Services;

public sealed class CategoryService(AppDbContext dbContext) : ICategoryService
{
    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedName = request.Name.Trim();

        var nameExists = await dbContext.Categories
            .AsNoTracking()
            .AnyAsync(
                x => x.UserId == request.UserId && x.Name.ToLower() == normalizedName.ToLower(),
                cancellationToken);

        if (nameExists)
        {
            throw new InvalidOperationException("Category with the same name already exists for this user.");
        }

        var category = new Category
        {
            UserId = request.UserId,
            Name = normalizedName,
            Description = request.Description?.Trim(),
            IsDefault = request.IsDefault,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CategoryDto(category.Id, category.UserId, category.Name, category.Description, category.IsDefault);
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Name)
            .Select(x => new CategoryDto(x.Id, x.UserId, x.Name, x.Description, x.IsDefault))
            .ToListAsync(cancellationToken);
    }
}
