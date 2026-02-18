using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Application.Abstractions.Services;
using PersonalFinanceTracker.Application.Contracts.Categories;
using PersonalFinanceTracker.Application.Exceptions;
using PersonalFinanceTracker.Domain.Entities;
using PersonalFinanceTracker.Infrastructure.Persistence;

namespace PersonalFinanceTracker.Infrastructure.Services;

public sealed class CategoryService(AppDbContext dbContext) : ICategoryService
{
    public async Task<CategoryDto> CreateAsync(Guid userId, CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedName = request.Name.Trim();
        var normalizedKey = normalizedName.ToUpperInvariant();

        var nameExists = await dbContext.Categories
            .AsNoTracking()
            .AnyAsync(
                x => x.UserId == userId && x.NormalizedName == normalizedKey,
                cancellationToken);

        if (nameExists)
        {
            throw new ConflictException("Category with the same name already exists for this user.");
        }

        var category = new Category
        {
            UserId = userId,
            Name = normalizedName,
            NormalizedName = normalizedKey,
            Description = request.Description?.Trim(),
            IsDefault = request.IsDefault,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CategoryDto(category.Id, category.UserId, category.Name, category.Description, category.IsDefault);
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(
        Guid userId,
        int page,
        int pageSize,
        string? sortBy,
        string? sortDirection,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            throw new ValidationException("page must be greater than or equal to 1.");
        }

        if (pageSize is < 1 or > 100)
        {
            throw new ValidationException("pageSize must be between 1 and 100.");
        }

        var normalizedSortBy = (sortBy ?? "name").Trim().ToLowerInvariant();
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.Categories
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        query = normalizedSortBy switch
        {
            "createdatutc" => descending ? query.OrderByDescending(x => x.CreatedAtUtc) : query.OrderBy(x => x.CreatedAtUtc),
            _ => descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name)
        };

        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new CategoryDto(x.Id, x.UserId, x.Name, x.Description, x.IsDefault))
            .ToListAsync(cancellationToken);
    }
}
