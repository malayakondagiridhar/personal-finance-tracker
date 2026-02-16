using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PersonalFinanceTracker.Application.Contracts.Categories;
using PersonalFinanceTracker.Application.Contracts.Transactions;
using PersonalFinanceTracker.Domain.Enums;
using PersonalFinanceTracker.IntegrationTests.TestHost;

namespace PersonalFinanceTracker.IntegrationTests;

public sealed class ApiErrorContractTests : IClassFixture<PersonalFinanceApiFactory>
{
    private readonly HttpClient _client;
    private readonly PersonalFinanceApiFactory _factory;

    public ApiErrorContractTests(PersonalFinanceApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task UpdateTransaction_NotFound_ShouldReturn404_WithStandardErrorPayload()
    {
        var userId = Guid.NewGuid();
        await TestDataSeeder.SeedUserAsync(_factory, userId);

        var categoryResponse = await _client.PostAsJsonAsync("/api/categories", new CreateCategoryRequest(userId, "Food", null, false));
        var category = await categoryResponse.Content.ReadFromJsonAsync<CategoryDto>();

        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);
        Assert.NotNull(category);

        var missingTransactionId = Guid.NewGuid();
        var updateRequest = new UpdateTransactionRequest(category!.Id, 500m, TransactionType.Expense, DateTime.UtcNow, "Update missing");

        var response = await _client.PutAsJsonAsync($"/api/transactions/{missingTransactionId}", updateRequest);

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
        await TestDataSeeder.SeedUserAsync(_factory, userId);

        var first = await _client.PostAsJsonAsync("/api/categories", new CreateCategoryRequest(userId, "Utilities", null, false));
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var duplicate = await _client.PostAsJsonAsync("/api/categories", new CreateCategoryRequest(userId, "utilities", null, false));

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
        await TestDataSeeder.SeedUserAsync(_factory, userId);

        var fromDate = DateTime.UtcNow.ToString("O");
        var toDate = DateTime.UtcNow.AddDays(-3).ToString("O");

        var response = await _client.GetAsync($"/api/transactions?userId={userId}&fromDateUtc={Uri.EscapeDataString(fromDate)}&toDateUtc={Uri.EscapeDataString(toDate)}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(400, payload.GetProperty("status").GetInt32());
        Assert.True(payload.TryGetProperty("title", out _));
        Assert.True(payload.TryGetProperty("detail", out _));
        Assert.True(payload.TryGetProperty("traceId", out _));
    }
}
