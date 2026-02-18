using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using PersonalFinanceTracker.Application.Contracts.Categories;
using PersonalFinanceTracker.Application.Contracts.Transactions;
using PersonalFinanceTracker.Domain.Enums;
using PersonalFinanceTracker.IntegrationTests.TestHost;

namespace PersonalFinanceTracker.IntegrationTests;

public sealed class ApiErrorContractTests : IClassFixture<PersonalFinanceApiFactory>
{
    private readonly PersonalFinanceApiFactory _factory;

    public ApiErrorContractTests(PersonalFinanceApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Request_WithoutToken_ShouldReturn401_WithStandardPayload()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/api/v1/categories");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(401, payload.GetProperty("status").GetInt32());
        Assert.True(payload.TryGetProperty("title", out _));
        Assert.True(payload.TryGetProperty("detail", out _));
        Assert.True(payload.TryGetProperty("traceId", out _));
    }

    [Fact]
    public async Task Request_WithoutScopeClaim_ShouldReturn403()
    {
        var userId = Guid.NewGuid();
        var client = AuthenticatedClientFactory.Create(_factory, userId, includeScope: false);

        var response = await client.GetAsync("/api/v1/categories");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Request_WithExpiredToken_ShouldReturn401_WithExpiredHeader()
    {
        var userId = Guid.NewGuid();
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

        var expiredToken = TestJwtTokenFactory.Create(
            userId,
            expiresAtUtc: DateTime.UtcNow.AddMinutes(-5),
            notBeforeUtc: DateTime.UtcNow.AddHours(-1));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", expiredToken);

        var response = await client.GetAsync("/api/v1/categories");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("x-token-expired", out var values));
        Assert.Contains("true", values!);

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(401, payload.GetProperty("status").GetInt32());
        Assert.Contains("expired", payload.GetProperty("detail").GetString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateTransaction_NotFound_ShouldReturn404_WithStandardErrorPayload()
    {
        var userId = Guid.NewGuid();
        var client = AuthenticatedClientFactory.Create(_factory, userId);

        var categoryResponse = await client.PostAsJsonAsync("/api/v1/categories", new CreateCategoryRequest("Food", null, false));
        var category = await categoryResponse.Content.ReadFromJsonAsync<CategoryDto>();

        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);
        Assert.NotNull(category);

        var missingTransactionId = Guid.NewGuid();
        var updateRequest = new UpdateTransactionRequest(category!.Id, 500m, TransactionType.Expense, DateTime.UtcNow, "Update missing");

        var response = await client.PutAsJsonAsync($"/api/v1/transactions/{missingTransactionId}", updateRequest);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(404, payload.GetProperty("status").GetInt32());
        Assert.True(payload.TryGetProperty("title", out _));
        Assert.True(payload.TryGetProperty("detail", out _));
        Assert.True(payload.TryGetProperty("traceId", out _));
    }

    [Fact]
    public async Task CreateCategory_Duplicate_ShouldReturn409_WithStandardErrorPayload()
    {
        var userId = Guid.NewGuid();
        var client = AuthenticatedClientFactory.Create(_factory, userId);

        var first = await client.PostAsJsonAsync("/api/v1/categories", new CreateCategoryRequest("Utilities", null, false));
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var duplicate = await client.PostAsJsonAsync("/api/v1/categories", new CreateCategoryRequest("utilities", null, false));

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        var payload = await duplicate.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(409, payload.GetProperty("status").GetInt32());
        Assert.True(payload.TryGetProperty("title", out _));
        Assert.True(payload.TryGetProperty("detail", out _));
        Assert.True(payload.TryGetProperty("traceId", out _));
    }

    [Fact]
    public async Task GetTransactions_InvalidDateRange_ShouldReturn400_WithStandardErrorPayload()
    {
        var userId = Guid.NewGuid();
        var client = AuthenticatedClientFactory.Create(_factory, userId);

        var fromDate = DateTime.UtcNow.ToString("O");
        var toDate = DateTime.UtcNow.AddDays(-3).ToString("O");

        var response = await client.GetAsync($"/api/v1/transactions?fromDateUtc={Uri.EscapeDataString(fromDate)}&toDateUtc={Uri.EscapeDataString(toDate)}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(400, payload.GetProperty("status").GetInt32());
        Assert.True(payload.TryGetProperty("title", out _));
        Assert.True(payload.TryGetProperty("detail", out _));
        Assert.True(payload.TryGetProperty("traceId", out _));
    }
}



