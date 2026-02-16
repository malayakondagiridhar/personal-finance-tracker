using System.Net.Http.Headers;

namespace PersonalFinanceTracker.IntegrationTests.TestHost;

public static class AuthenticatedClientFactory
{
    public static HttpClient Create(PersonalFinanceApiFactory factory, Guid userId, bool includeScope = true)
    {
        var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

        var token = TestJwtTokenFactory.Create(userId, includeScope);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
