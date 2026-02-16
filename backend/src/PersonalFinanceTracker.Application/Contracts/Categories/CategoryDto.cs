namespace PersonalFinanceTracker.Application.Contracts.Categories;

public sealed record CategoryDto(
    Guid Id,
    Guid UserId,
    string Name,
    string? Description,
    bool IsDefault);
