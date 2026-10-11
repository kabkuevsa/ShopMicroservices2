using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OrderService.Clients;

namespace OrderService.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<IUserServiceClient> UserClientMock { get; } = new();
    public Mock<IProductServiceClient> ProductClientMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.AddScoped(_ => UserClientMock.Object);
            services.AddScoped(_ => ProductClientMock.Object);
        });
    }
}