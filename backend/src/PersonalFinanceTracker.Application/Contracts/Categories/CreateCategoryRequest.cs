namespace PersonalFinanceTracker.Application.Contracts.Categories;

public sealed record CreateCategoryRequest(
    string Name,
    string? Description,
    bool IsDefault);
