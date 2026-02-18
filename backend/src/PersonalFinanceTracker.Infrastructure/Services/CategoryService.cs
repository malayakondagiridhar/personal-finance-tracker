using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Application.Abstractions.Services;
using PersonalFinanceTracker.Application.Contracts.Categories;
using PersonalFinanceTracker.Application.Contracts.Common;
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

    public async Task<PagedResult<CategoryDto>> GetAllAsync(
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

        var baseQuery = dbContext.Categories
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var query = normalizedSortBy switch
        {
            "createdatutc" => descending ? baseQuery.OrderByDescending(x => x.CreatedAtUtc) : baseQuery.OrderBy(x => x.CreatedAtUtc),
            _ => descending ? baseQuery.OrderByDescending(x => x.Name) : baseQuery.OrderBy(x => x.Name)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new CategoryDto(x.Id, x.UserId, x.Name, x.Description, x.IsDefault))
            .ToListAsync(cancellationToken);

        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PagedResult<CategoryDto>(items, page, pageSize, totalCount, totalPages);
    }
}
