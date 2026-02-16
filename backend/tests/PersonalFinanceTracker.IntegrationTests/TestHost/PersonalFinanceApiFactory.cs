using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PersonalFinanceTracker.IntegrationTests.TestHost;

public sealed class PersonalFinanceApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var databaseName = $"PersonalFinanceTrackerTests_{Guid.NewGuid():N}";

        builder.UseSetting("DatabaseProvider", "InMemory");
        builder.UseSetting("InMemoryDatabaseName", databaseName);
    }
}
