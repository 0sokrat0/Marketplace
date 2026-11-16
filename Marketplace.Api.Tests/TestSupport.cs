using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Marketplace.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace Marketplace.Api.Tests;

public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"marketplace-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.UseSetting("ConnectionStrings:Default", $"Data Source={_databasePath}");

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        SqliteConnection.ClearAllPools();
        foreach (var file in Directory.GetFiles(Path.GetTempPath(), Path.GetFileName(_databasePath) + "*"))
        {
            File.Delete(file);
        }
    }
}

public record Result(HttpStatusCode Status, string Body)
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public T Json<T>() => JsonSerializer.Deserialize<T>(Body, Options)!;

    public string Error => Body;
}

public class Api(HttpClient http)
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public async Task<Api> SignInAs(string login, string password)
    {
        var result = await Post("api/Auth/Login", new LoginRequest(login, password));
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Json<AuthResponse>().Token);
        return this;
    }

    public async Task<Api> SignUp(string login, string password = "secret1")
    {
        var result = await Post("api/Auth/Register", new RegisterRequest(login, password, password));
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Json<AuthResponse>().Token);
        return this;
    }

    public Task<Result> Get(string url) => Send(HttpMethod.Get, url);

    public Task<Result> Post(string url, object? body = null) => Send(HttpMethod.Post, url, body);

    public Task<Result> Put(string url, object? body = null) => Send(HttpMethod.Put, url, body);

    public Task<Result> Delete(string url) => Send(HttpMethod.Delete, url);

    private async Task<Result> Send(HttpMethod method, string url, object? body = null)
    {
        using var request = new HttpRequestMessage(method, url);
        if (body is not null)
        {
            request.Content = new StringContent(JsonSerializer.Serialize(body, Options), Encoding.UTF8, "application/json");
        }

        using var response = await http.SendAsync(request);
        return new Result(response.StatusCode, await response.Content.ReadAsStringAsync());
    }
}

public abstract class ApiTestBase : IDisposable
{
    protected const int Mug = 5;
    protected const int Cable = 9;
    protected const int Speaker = 10;
    protected const int OutOfStock = 12;
    protected const int Withdrawn = 13;

    protected ApiFactory Factory { get; } = new();

    protected Api Guest() => new(Factory.CreateClient());

    protected Task<Api> User(string login = "buyer") => new Api(Factory.CreateClient()).SignUp(login);

    protected Task<Api> Admin() => new Api(Factory.CreateClient()).SignInAs("admin", "admin123");

    protected static AddToCartRequest ToCart(int productId, int quantity = 1) => new(productId, quantity);

    protected static CreateOrderRequest PickupOrder(int productId, int quantity = 1) =>
        new(DeliveryMethods.Pickup, null, 1, new OrderItemRequest(productId, quantity));

    protected static ProductRequest ProductWithPrice(string name, decimal price, int categoryId, int stock, bool available = true) =>
        new(name, "Описание", price, "item.png", categoryId, stock, null, available);

    public void Dispose() => Factory.Dispose();
}
