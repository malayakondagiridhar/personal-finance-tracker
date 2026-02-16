using PersonalFinanceTracker.Domain.Common;

namespace PersonalFinanceTracker.Domain.Entities;

public sealed class User : BaseEntity
{
    public string ExternalAuthId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
}
