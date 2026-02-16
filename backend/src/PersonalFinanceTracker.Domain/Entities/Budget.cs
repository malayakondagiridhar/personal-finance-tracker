using PersonalFinanceTracker.Domain.Common;

namespace PersonalFinanceTracker.Domain.Entities;

public sealed class Budget : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid CategoryId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal LimitAmount { get; set; }

    public User? User { get; set; }
    public Category? Category { get; set; }
}
