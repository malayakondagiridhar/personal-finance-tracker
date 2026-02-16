using PersonalFinanceTracker.Application.Contracts.Categories;

namespace PersonalFinanceTracker.Application.Abstractions.Services;

public interface ICategoryService
{
    Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default);
}
