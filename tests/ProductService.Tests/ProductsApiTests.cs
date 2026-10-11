using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProductService.Models.DTOs;
using Xunit;

namespace ProductService.Tests;

public class ProductsApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductsApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreated()
    {
        var request = new CreateProductRequest
        {
            Name = "Ноутбук " + Guid.NewGuid().ToString("N")[..6],
            Description = "15 дюймов",
            Category = "Electronics",
            Price = 75000,
            Stock = 10
        };

        var response = await _client.PostAsJsonAsync("/api/products", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ProductDto>();
        result.Should().NotBeNull();
        result!.Name.Should().Be(request.Name);
        result.Price.Should().Be(75000);
        result.Stock.Should().Be(10);
    }

    [Fact]
    public async Task GetAll_ReturnsList()
    {
        var response = await _client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
        list.Should().NotBeNull();
    }

    [Fact]
    public async Task GetById_ExistingProduct_ReturnsProduct()
    {
        var create = new CreateProductRequest
        {
            Name = "Товар " + Guid.NewGuid().ToString("N")[..6],
            Description = "X",
            Category = "Test",
            Price = 100,
            Stock = 5
        };
        var createResponse = await _client.PostAsJsonAsync("/api/products", create);
        var created = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

        var response = await _client.GetAsync($"/api/products/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        product!.Name.Should().Be(create.Name);
    }

    [Fact]
    public async Task GetById_NotExisting_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/products/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Reserve_DecreasesStock()
    {
        var create = new CreateProductRequest
        {
            Name = "Резерв " + Guid.NewGuid().ToString("N")[..6],
            Description = "X",
            Category = "Test",
            Price = 100,
            Stock = 10
        };
        var createResponse = await _client.PostAsJsonAsync("/api/products", create);
        var created = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

        var reserveResponse = await _client.PostAsync(
            $"/api/products/{created!.Id}/reserve?quantity=3", null);

        reserveResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/products/{created.Id}");
        var product = await getResponse.Content.ReadFromJsonAsync<ProductDto>();
        product!.Stock.Should().Be(7);
    }

    [Fact]
    public async Task Reserve_NotEnoughStock_ReturnsBadRequest()
    {
        var create = new CreateProductRequest
        {
            Name = "Мало " + Guid.NewGuid().ToString("N")[..6],
            Description = "X",
            Category = "Test",
            Price = 100,
            Stock = 2
        };
        var createResponse = await _client.PostAsJsonAsync("/api/products", create);
        var created = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

        var response = await _client.PostAsync(
            $"/api/products/{created!.Id}/reserve?quantity=10", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAll_WithCategoryFilter_ReturnsFiltered()
    {
        var category = "Filter_" + Guid.NewGuid().ToString("N")[..6];
        await _client.PostAsJsonAsync("/api/products", new CreateProductRequest
        {
            Name = "A",
            Description = "X",
            Category = category,
            Price = 100,
            Stock = 1
        });

        var response = await _client.GetAsync($"/api/products?category={category}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
        list.Should().NotBeNull();
        list!.Should().AllSatisfy(p => p.Category.Should().Be(category));
    }
}