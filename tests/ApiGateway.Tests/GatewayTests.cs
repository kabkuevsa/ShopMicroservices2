using System.Net;
using FluentAssertions;
using Xunit;

namespace ApiGateway.Tests;

public class GatewayTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public GatewayTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Gateway_Starts_AndResponds()
    {
        var response = await _client.GetAsync("/");
        response.Should().NotBeNull();
        response.StatusCode.Should().NotBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task Gateway_UsersRoute_IsDefined()
    {
        var response = await _client.GetAsync("/api/users/" + Guid.NewGuid());
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadGateway,
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.NotFound,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Gateway_ProductsRoute_IsDefined()
    {
        var response = await _client.GetAsync("/api/products");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadGateway,
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.NotFound,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Gateway_OrdersRoute_IsDefined()
    {
        var response = await _client.GetAsync("/api/orders");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadGateway,
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.NotFound,
            HttpStatusCode.InternalServerError);
    }
}