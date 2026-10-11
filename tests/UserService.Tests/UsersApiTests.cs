using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using UserService.Models.DTOs;

namespace UserService.Tests;

public class UsersApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UsersApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    //  Тест 1. Успешная регистрация
    [Fact]
    public async Task Register_WithValidData_ReturnsOk()
    {
        var request = new RegisterRequest
        {
            Email = $"user_{Guid.NewGuid():N}@test.com",
            Password = "123456",
            FullName = "Иван Петров"
        };

        var response = await _client.PostAsJsonAsync("/api/users/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        result.Should().NotBeNull();
        result!.Email.Should().Be(request.Email);
        result.FullName.Should().Be("Иван Петров");
        result.Id.Should().NotBeEmpty();
    }

    //  Тест 2. Дубликат email
    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        var email = $"dup_{Guid.NewGuid():N}@test.com";
        var request = new RegisterRequest { Email = email, Password = "123456", FullName = "A" };

        await _client.PostAsJsonAsync("/api/users/register", request);
        var secondResponse = await _client.PostAsJsonAsync("/api/users/register", request);

        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    //  Тест 3. Успешный логин
    [Fact]
    public async Task Login_WithCorrectPassword_ReturnsOk()
    {
        var email = $"login_{Guid.NewGuid():N}@test.com";
        await _client.PostAsJsonAsync("/api/users/register", new RegisterRequest
        {
            Email = email,
            Password = "secret",
            FullName = "Пётр"
        });

        var loginResponse = await _client.PostAsJsonAsync("/api/users/login",
            new LoginRequest { Email = email, Password = "secret" });

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        result!.Email.Should().Be(email);
    }

    //  Тест 4. Логин с неправильным паролем
    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var email = $"wrong_{Guid.NewGuid():N}@test.com";
        await _client.PostAsJsonAsync("/api/users/register", new RegisterRequest
        {
            Email = email,
            Password = "correct",
            FullName = "X"
        });

        var response = await _client.PostAsJsonAsync("/api/users/login",
            new LoginRequest { Email = email, Password = "wrong" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    //  Тест 5. Получение юзера по ID
    [Fact]
    public async Task GetById_ExistingUser_ReturnsUser()
    {
        var email = $"get_{Guid.NewGuid():N}@test.com";
        var registerResponse = await _client.PostAsJsonAsync("/api/users/register",
            new RegisterRequest { Email = email, Password = "123", FullName = "Тест" });
        var created = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        var response = await _client.GetAsync($"/api/users/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user!.Email.Should().Be(email);
    }

    //  Тест 6. Получение несуществующего юзера
    [Fact]
    public async Task GetById_NotExisting_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/users/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}