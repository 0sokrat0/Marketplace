using System.Net;
using Marketplace.Api.Entities;
using Marketplace.Api.Infrastructure;
using Marketplace.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Marketplace.Api.Tests;

public class AuthTests : ApiTestBase
{
    [Fact]
    public async Task Register_ExistingUserWithSamePassword_SignsIn()
    {
        var guest = Guest();
        await guest.Post("api/auth/register", new RegisterRequest("alice", "secret1", "secret1"));

        var again = await guest.Post("api/auth/register", new RegisterRequest("alice", "secret1", "secret1"));

        Assert.Equal(HttpStatusCode.OK, again.Status);
        Assert.Equal("alice", again.Json<AuthResponse>().Login);
    }

    [Fact]
    public async Task Register_ExistingUserWithWrongPassword_IsRejected()
    {
        var guest = Guest();
        await guest.Post("api/auth/register", new RegisterRequest("alice", "secret1", "secret1"));

        var result = await guest.Post("api/auth/register", new RegisterRequest("alice", "another1", "another1"));

        Assert.Equal(HttpStatusCode.Unauthorized, result.Status);
    }

    [Fact]
    public async Task Register_PasswordsDiffer_IsRejected()
    {
        var result = await Guest().Post("api/auth/register", new RegisterRequest("bob", "secret1", "secret2"));

        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
        Assert.Equal("Пароли не совпадают", result.Error);
    }

    [Fact]
    public async Task Login_WrongPassword_IsRejected()
    {
        var result = await Guest().Post("api/auth/login", new LoginRequest("admin", "wrong"));

        Assert.Equal(HttpStatusCode.Unauthorized, result.Status);
    }

    [Fact]
    public async Task NewUser_GetsUserRole_AndPasswordIsStoredAsHash()
    {
        var result = await Guest().Post("api/auth/register", new RegisterRequest("carol", "secret1", "secret1"));

        Assert.Equal(Roles.User, result.Json<AuthResponse>().Role);
        using var scope = Factory.Services.CreateScope();
        var user = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>().Users.Single(u => u.Login == "carol");
        Assert.NotEqual("secret1", user.PasswordHash);
        Assert.DoesNotContain("secret1", user.PasswordHash);
    }
}
