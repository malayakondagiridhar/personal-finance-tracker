using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PersonalFinanceTracker.Application.Contracts.Categories;
using PersonalFinanceTracker.IntegrationTests.TestHost;

namespace PersonalFinanceTracker.IntegrationTests;

public sealed class CategoriesApiTests : IClassFixture<PersonalFinanceApiFactory>
{
    private readonly HttpClient _client;
    private readonly PersonalFinanceApiFactory _factory;

    public CategoriesApiTests(PersonalFinanceApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Create_Then_GetAll_ShouldReturnCreatedCategory()
    {
        var userId = Guid.NewGuid();
        await TestDataSeeder.SeedUserAsync(_factory, userId);

        var createRequest = new CreateCategoryRequest(userId, "Food", "Groceries", false);
        var createResponse = await _client.PostAsJsonAsync("/api/categories", createRequest);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var listResponse = await _client.GetAsync($"/api/categories?userId={userId}");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var categories = await listResponse.Content.ReadFromJsonAsync<List<CategoryDto>>();
        Assert.NotNull(categories);
        Assert.Single(categories!);
        Assert.Equal("Food", categories[0].Name);
    }

    [Fact]
    public async Task Create_DuplicateNameForSameUser_ShouldReturnConflict()
    {
        var userId = Guid.NewGuid();
        await TestDataSeeder.SeedUserAsync(_factory, userId);

        var request = new CreateCategoryRequest(userId, "Utilities", null, false);

        var firstResponse = await _client.PostAsJsonAsync("/api/categories", request);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var secondResponse = await _client.PostAsJsonAsync("/api/categories", new CreateCategoryRequest(userId, "utilities", null, false));
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);

        var payload = await secondResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(409, payload.GetProperty("status").GetInt32());
    }
}
