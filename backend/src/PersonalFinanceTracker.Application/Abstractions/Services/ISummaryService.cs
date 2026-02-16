using PersonalFinanceTracker.Application.Contracts.Summaries;

namespace PersonalFinanceTracker.Application.Abstractions.Services;

public interface ISummaryService
{
    Task<MonthlySummaryDto> GetMonthlySummaryAsync(Guid userId, int year, int month, CancellationToken cancellationToken = default);
}
