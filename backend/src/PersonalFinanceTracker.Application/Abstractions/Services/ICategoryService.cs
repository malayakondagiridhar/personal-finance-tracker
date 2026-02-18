using PersonalFinanceTracker.Application.Contracts.Categories;

namespace PersonalFinanceTracker.Application.Abstractions.Services;

public interface ICategoryService
{
    Task<CategoryDto> CreateAsync(Guid userId, CreateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(
        Guid userId,
        int page,
        int pageSize,
        string? sortBy,
        string? sortDirection,
        CancellationToken cancellationToken = default);
}
