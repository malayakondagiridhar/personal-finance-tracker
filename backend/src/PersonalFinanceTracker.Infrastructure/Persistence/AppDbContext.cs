using Microsoft.EntityFrameworkCore;

namespace PersonalFinanceTracker.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
}
