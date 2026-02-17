using PersonalFinanceTracker.Application.Contracts.Budgets;

namespace PersonalFinanceTracker.Application.Abstractions.Services;

public interface IBudgetService
{
    Task<BudgetDto> CreateAsync(Guid userId, CreateBudgetRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BudgetStatusDto>> GetMonthlyStatusAsync(Guid userId, int year, int month, CancellationToken cancellationToken = default);
}
