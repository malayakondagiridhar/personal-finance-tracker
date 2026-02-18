using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PersonalFinanceTracker.Application.Contracts.Categories;
using PersonalFinanceTracker.Application.Contracts.Transactions;
using PersonalFinanceTracker.Domain.Enums;
using PersonalFinanceTracker.IntegrationTests.TestHost;

namespace PersonalFinanceTracker.IntegrationTests;

public sealed class TransactionsApiTests : IClassFixture<PersonalFinanceApiFactory>
{
    private readonly PersonalFinanceApiFactory _factory;

    public TransactionsApiTests(PersonalFinanceApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_Then_GetWithFilters_ShouldReturnTransaction()
    {
        var userId = Guid.NewGuid();
        var client = AuthenticatedClientFactory.Create(_factory, userId);

        var categoryResponse = await client.PostAsJsonAsync("/api/v1/categories", new CreateCategoryRequest("Travel", "Cab and fuel", false));
        var createdCategory = await categoryResponse.Content.ReadFromJsonAsync<CategoryDto>();

        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);
        Assert.NotNull(createdCategory);

        var transactionRequest = new CreateTransactionRequest(
            createdCategory!.Id,
            1200m,
            TransactionType.Expense,
            DateTime.UtcNow,
            "Airport cab");

        var createTransactionResponse = await client.PostAsJsonAsync("/api/v1/transactions", transactionRequest);
        Assert.Equal(HttpStatusCode.Created, createTransactionResponse.StatusCode);

        var fromDate = DateTime.UtcNow.AddDays(-1).ToString("O");
        var toDate = DateTime.UtcNow.AddDays(1).ToString("O");

        var getResponse = await client.GetAsync($"/api/v1/transactions?fromDateUtc={Uri.EscapeDataString(fromDate)}&toDateUtc={Uri.EscapeDataString(toDate)}&type={(int)TransactionType.Expense}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var transactions = await getResponse.Content.ReadFromJsonAsync<List<TransactionDto>>();
        Assert.NotNull(transactions);
        Assert.Single(transactions!);
        Assert.Equal(1200m, transactions[0].Amount);
        Assert.Equal(userId, transactions[0].UserId);
    }

    [Fact]
    public async Task Get_WithInvalidDateRange_ShouldReturnBadRequest()
    {
        var userId = Guid.NewGuid();
        var client = AuthenticatedClientFactory.Create(_factory, userId);

        var fromDate = DateTime.UtcNow.ToString("O");
        var toDate = DateTime.UtcNow.AddDays(-2).ToString("O");

        var response = await client.GetAsync($"/api/v1/transactions?fromDateUtc={Uri.EscapeDataString(fromDate)}&toDateUtc={Uri.EscapeDataString(toDate)}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(400, payload.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task Get_WithPaginationSortAndInvalidPageSize_ShouldRespectContract()
    {
        var userId = Guid.NewGuid();
        var client = AuthenticatedClientFactory.Create(_factory, userId);

        var categoryResponse = await client.PostAsJsonAsync("/api/v1/categories", new CreateCategoryRequest("Food", null, false));
        var category = await categoryResponse.Content.ReadFromJsonAsync<CategoryDto>();
        Assert.NotNull(category);

        await client.PostAsJsonAsync("/api/v1/transactions", new CreateTransactionRequest(category!.Id, 100m, TransactionType.Expense, DateTime.UtcNow.AddMinutes(-3), "t1"));
        await client.PostAsJsonAsync("/api/v1/transactions", new CreateTransactionRequest(category.Id, 250m, TransactionType.Expense, DateTime.UtcNow.AddMinutes(-2), "t2"));
        await client.PostAsJsonAsync("/api/v1/transactions", new CreateTransactionRequest(category.Id, 400m, TransactionType.Expense, DateTime.UtcNow.AddMinutes(-1), "t3"));

        var pagedResponse = await client.GetAsync("/api/v1/transactions?page=1&pageSize=2&sortBy=amount&sortDirection=desc");
        Assert.Equal(HttpStatusCode.OK, pagedResponse.StatusCode);

        var paged = await pagedResponse.Content.ReadFromJsonAsync<List<TransactionDto>>();
        Assert.NotNull(paged);
        Assert.Equal(2, paged!.Count);
        Assert.Equal(400m, paged[0].Amount);
        Assert.Equal(250m, paged[1].Amount);

        var invalidPageSizeResponse = await client.GetAsync("/api/v1/transactions?page=1&pageSize=101");
        Assert.Equal(HttpStatusCode.BadRequest, invalidPageSizeResponse.StatusCode);

        var payload = await invalidPageSizeResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(400, payload.GetProperty("status").GetInt32());
    }
}

