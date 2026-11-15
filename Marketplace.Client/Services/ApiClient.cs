using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Marketplace.Contracts;

namespace Marketplace.Client.Services;

public class ApiClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _http;

    public ApiClient(string baseAddress)
    {
        _http = new HttpClient { BaseAddress = new Uri(baseAddress) };
    }

    public void SetToken(string? token) =>
        _http.DefaultRequestHeaders.Authorization = token is null ? null : new AuthenticationHeaderValue("Bearer", token);

    public Task<AuthResponse> Register(RegisterRequest request) => Send<AuthResponse>(HttpMethod.Post, "api/Auth/Register", request);

    public Task<AuthResponse> Login(LoginRequest request) => Send<AuthResponse>(HttpMethod.Post, "api/Auth/Login", request);

    public Task<List<CategoryDto>> GetCategories() => Send<List<CategoryDto>>(HttpMethod.Get, "api/Category");

    public Task<List<ProductDto>> GetProducts(ProductFilter filter)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query.Add("search=" + Uri.EscapeDataString(filter.Search.Trim()));
        }

        if (filter.CategoryId is not null)
        {
            query.Add("categoryId=" + filter.CategoryId);
        }

        if (filter.MinPrice is not null)
        {
            query.Add("minPrice=" + filter.MinPrice.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (filter.MaxPrice is not null)
        {
            query.Add("maxPrice=" + filter.MaxPrice.Value.ToString(CultureInfo.InvariantCulture));
        }

        var url = query.Count == 0 ? "api/Product" : "api/Product?" + string.Join('&', query);
        return Send<List<ProductDto>>(HttpMethod.Get, url);
    }

    public Task<ProductDto> GetProduct(int id) => Send<ProductDto>(HttpMethod.Get, $"api/Product/{id}");

    public Task<List<ReviewDto>> GetReviews(int productId) =>
        Send<List<ReviewDto>>(HttpMethod.Get, $"api/Review/Product/{productId}");

    public Task<ReviewDto> AddReview(int productId, ReviewRequest request) =>
        Send<ReviewDto>(HttpMethod.Post, $"api/Review/Product/{productId}", request);

    public Task<List<PickupPointDto>> GetPickupPoints() => Send<List<PickupPointDto>>(HttpMethod.Get, "api/PickupPoint");

    public Task<CartDto> GetCart() => Send<CartDto>(HttpMethod.Get, "api/Cart");

    public Task<CartDto> AddToCart(AddToCartRequest request) => Send<CartDto>(HttpMethod.Post, "api/Cart/Items", request);

    public Task<CartDto> SetCartQuantity(int productId, int quantity) =>
        Send<CartDto>(HttpMethod.Put, $"api/Cart/Items/{productId}", new QuantityRequest(quantity));

    public Task<CartDto> RemoveFromCart(int productId) => Send<CartDto>(HttpMethod.Delete, $"api/Cart/Items/{productId}");

    public Task<List<ProductDto>> GetFavorites() => Send<List<ProductDto>>(HttpMethod.Get, "api/Favorite");

    public Task AddFavorite(int productId) => SendEmpty(HttpMethod.Post, $"api/Favorite/{productId}");

    public Task RemoveFavorite(int productId) => SendEmpty(HttpMethod.Delete, $"api/Favorite/{productId}");

    public Task<CartDto> MoveFavoriteToCart(int productId) =>
        Send<CartDto>(HttpMethod.Post, $"api/Favorite/{productId}/ToCart");

    public Task<OrderDto> CreateOrder(CreateOrderRequest request) => Send<OrderDto>(HttpMethod.Post, "api/Order", request);

    public Task<List<OrderSummaryDto>> GetOrders(bool newestFirst) =>
        Send<List<OrderSummaryDto>>(HttpMethod.Get, $"api/Order/My?sort={(newestFirst ? "new" : "old")}");

    public Task<OrderDto> GetOrder(int id) => Send<OrderDto>(HttpMethod.Get, $"api/Order/My/{id}");

    public Task<ProductDto> CreateProduct(ProductRequest request) => Send<ProductDto>(HttpMethod.Post, "api/Product", request);

    public Task<ProductDto> UpdateProduct(int id, ProductRequest request) =>
        Send<ProductDto>(HttpMethod.Put, $"api/Product/{id}", request);

    public Task<CategoryDto> CreateCategory(CategoryRequest request) =>
        Send<CategoryDto>(HttpMethod.Post, "api/Category", request);

    public Task<CategoryDto> UpdateCategory(int id, CategoryRequest request) =>
        Send<CategoryDto>(HttpMethod.Put, $"api/Category/{id}", request);

    public Task<PickupPointDto> CreatePickupPoint(PickupPointRequest request) =>
        Send<PickupPointDto>(HttpMethod.Post, "api/PickupPoint", request);

    public Task<PickupPointDto> UpdatePickupPoint(int id, PickupPointRequest request) =>
        Send<PickupPointDto>(HttpMethod.Put, $"api/PickupPoint/{id}", request);

    public Task DeletePickupPoint(int id) => SendEmpty(HttpMethod.Delete, $"api/PickupPoint/{id}");

    public Task<List<OrderSummaryDto>> GetAllOrders() => Send<List<OrderSummaryDto>>(HttpMethod.Get, "api/Order");

    public Task<OrderDto> GetAnyOrder(int id) => Send<OrderDto>(HttpMethod.Get, $"api/Order/{id}");

    public Task<OrderDto> SetOrderStatus(int id, string status) =>
        Send<OrderDto>(HttpMethod.Put, $"api/Order/{id}/Status", new StatusRequest(status));

    public Task DeleteReview(int id) => SendEmpty(HttpMethod.Delete, $"api/Review/{id}");

    private async Task<T> Send<T>(HttpMethod method, string url, object? body = null)
    {
        using var response = await Execute(method, url, body);
        return (await response.Content.ReadFromJsonAsync<T>(Json))!;
    }

    private async Task SendEmpty(HttpMethod method, string url, object? body = null)
    {
        using var response = await Execute(method, url, body);
    }

    private async Task<HttpResponseMessage> Execute(HttpMethod method, string url, object? body)
    {
        using var request = new HttpRequestMessage(method, url);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: Json);
        }

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request);
        }
        catch (HttpRequestException)
        {
            throw new ApiException("Не удалось связаться с сервером");
        }

        if (!response.IsSuccessStatusCode)
        {
            var message = await ReadError(response);
            response.Dispose();
            throw new ApiException(message);
        }

        return response;
    }

    private static async Task<string> ReadError(HttpResponseMessage response)
    {
        var message = await response.Content.ReadAsStringAsync();
        if (!string.IsNullOrWhiteSpace(message))
        {
            return message;
        }

        return response.StatusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized => "Нужно войти в аккаунт",
            System.Net.HttpStatusCode.Forbidden => "Недостаточно прав",
            _ => $"Ошибка сервера: {(int)response.StatusCode}"
        };
    }
}
