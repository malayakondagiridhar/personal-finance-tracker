using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonalFinanceTracker.Application.Abstractions.Services;
using PersonalFinanceTracker.Infrastructure.Persistence;
using PersonalFinanceTracker.Infrastructure.Services;

namespace PersonalFinanceTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseProvider = configuration["DatabaseProvider"];

        if (string.Equals(databaseProvider, "InMemory", StringComparison.OrdinalIgnoreCase))
        {
            var inMemoryDatabaseName = configuration["InMemoryDatabaseName"] ?? "PersonalFinanceTrackerTestDb";
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(inMemoryDatabaseName));
        }
        else
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
        }

        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IBudgetService, BudgetService>();
        services.AddScoped<ISummaryService, SummaryService>();

        return services;
    }
}
