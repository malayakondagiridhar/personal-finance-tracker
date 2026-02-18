using PersonalFinanceTracker.Domain.Common;
using PersonalFinanceTracker.Domain.Enums;

namespace PersonalFinanceTracker.Domain.Entities;

public sealed class Transaction : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid CategoryId { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public DateTime TransactionDateUtc { get; set; }
    public string? Note { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    public User? User { get; set; }
    public Category? Category { get; set; }
}
