namespace PersonalFinanceTracker.Application.Contracts.Categories;

public sealed record CreateCategoryRequest(
    Guid UserId,
    string Name,
    string? Description,
    bool IsDefault);
