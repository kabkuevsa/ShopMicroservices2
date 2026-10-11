using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using OrderService.Clients;
using OrderService.Models.DTOs;
using Xunit;

namespace OrderService.Tests;

public class OrdersApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public OrdersApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_WithValidUserAndProduct_ReturnsOrderWithDetails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        _factory.UserClientMock
            .Setup(c => c.GetUserAsync(userId))
            .ReturnsAsync(new UserDto { Id = userId, Email = "a@b.com", FullName = "Иван" });

        _factory.ProductClientMock
            .Setup(c => c.GetProductAsync(productId))
            .ReturnsAsync(new ProductDto { Id = productId, Name = "Ноутбук", Price = 75000, Stock = 10 });

        _factory.ProductClientMock
            .Setup(c => c.ReserveProductAsync(productId, 2))
            .Returns(Task.CompletedTask);

        var request = new CreateOrderRequest { UserId = userId, ProductId = productId, Quantity = 2 };

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        order.Should().NotBeNull();
        order!.UserId.Should().Be(userId);
        order.ProductId.Should().Be(productId);
        order.Quantity.Should().Be(2);
        order.TotalPrice.Should().Be(150000);   // 75000 * 2
        order.User.Should().NotBeNull();
        order.Product.Should().NotBeNull();

        _factory.ProductClientMock.Verify(c => c.ReserveProductAsync(productId, 2), Times.Once);
    }

    [Fact]
    public async Task Create_WhenUserNotFound_ReturnsBadRequest()
    {
        var userId = Guid.NewGuid();
        _factory.UserClientMock
            .Setup(c => c.GetUserAsync(userId))
            .ReturnsAsync((UserDto?)null);

        var request = new CreateOrderRequest { UserId = userId, ProductId = Guid.NewGuid(), Quantity = 1 };

        var response = await _client.PostAsJsonAsync("/api/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WhenProductNotFound_ReturnsBadRequest()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        _factory.UserClientMock
            .Setup(c => c.GetUserAsync(userId))
            .ReturnsAsync(new UserDto { Id = userId, Email = "a@b.com", FullName = "X" });

        _factory.ProductClientMock
            .Setup(c => c.GetProductAsync(productId))
            .ReturnsAsync((ProductDto?)null);

        var request = new CreateOrderRequest { UserId = userId, ProductId = productId, Quantity = 1 };

        var response = await _client.PostAsJsonAsync("/api/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WhenNotEnoughStock_ReturnsBadRequest()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        _factory.UserClientMock
            .Setup(c => c.GetUserAsync(userId))
            .ReturnsAsync(new UserDto { Id = userId, Email = "a@b.com", FullName = "X" });

        _factory.ProductClientMock
            .Setup(c => c.GetProductAsync(productId))
            .ReturnsAsync(new ProductDto { Id = productId, Name = "X", Price = 100, Stock = 1 });

        var request = new CreateOrderRequest { UserId = userId, ProductId = productId, Quantity = 10 };

        var response = await _client.PostAsJsonAsync("/api/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WhenUserServiceUnavailable_ReturnsBadRequest()
    {
        var userId = Guid.NewGuid();

        _factory.UserClientMock
            .Setup(c => c.GetUserAsync(userId))
            .ThrowsAsync(new InvalidOperationException("UserService недоступен"));

        var request = new CreateOrderRequest { UserId = userId, ProductId = Guid.NewGuid(), Quantity = 1 };

        var response = await _client.PostAsJsonAsync("/api/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAll_ReturnsList()
    {
        var response = await _client.GetAsync("/api/orders");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_NotExisting_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/orders/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}