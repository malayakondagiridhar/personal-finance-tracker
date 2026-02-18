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

        builder.UseSetting("Auth:FirebaseProjectId", string.Empty);
        builder.UseSetting("Auth:Issuer", TestAuthDefaults.Issuer);
        builder.UseSetting("Auth:Audience", TestAuthDefaults.Audience);
        builder.UseSetting("Auth:SigningKey", TestAuthDefaults.SigningKey);
    }
}
