using System.Net;
using Marketplace.Contracts;

namespace Marketplace.Api.Tests;

public class AdminTests : ApiTestBase
{
    [Fact]
    public async Task OrdinaryUser_CannotUseAdminEndpoints()
    {
        var user = await User();

        Assert.Equal(HttpStatusCode.Forbidden,
            (await user.Post("api/admin/products", ProductWithPrice("Взлом", 1, 1, 1))).Status);
        Assert.Equal(HttpStatusCode.Forbidden, (await user.Get("api/admin/orders")).Status);
        Assert.Equal(HttpStatusCode.Forbidden, (await user.Delete("api/admin/reviews/1")).Status);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await user.Post("api/admin/categories", new CategoryRequest("Взлом"))).Status);
    }

    [Fact]
    public async Task Admin_CreatesAndEditsProduct_AndMakesItUnavailable()
    {
        var admin = await Admin();
        var user = await User();

        var created = (await admin.Post("api/admin/products", ProductWithPrice("Новая лампа", 499.5m, 2, 10))).Json<ProductDto>();
        Assert.Equal(HttpStatusCode.OK, (await user.Post("api/cart/items", ToCart(created.Id))).Status);

        var edited = (await admin.Put($"api/admin/products/{created.Id}",
            ProductWithPrice("Новая лампа", 550m, 2, 10, available: false))).Json<ProductDto>();

        Assert.Equal(550m, edited.Price);
        Assert.False(edited.CanBuy);
        Assert.Equal(HttpStatusCode.BadRequest, (await user.Post("api/cart/items", ToCart(created.Id))).Status);
    }

    [Fact]
    public async Task Admin_ChangesStock()
    {
        var admin = await Admin();

        var edited = (await admin.Put($"api/admin/products/{OutOfStock}",
            ProductWithPrice("Коврик для йоги", 1150, 4, 7))).Json<ProductDto>();

        Assert.Equal(7, edited.Stock);
        Assert.True(edited.CanBuy);
    }

    [Fact]
    public async Task InvalidProductData_IsRejected()
    {
        var admin = await Admin();

        Assert.Equal(HttpStatusCode.BadRequest, (await admin.Post("api/admin/products", ProductWithPrice("Минус", -1, 1, 1))).Status);
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.Post("api/admin/products", ProductWithPrice("Склад", 1, 1, -1))).Status);
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.Post("api/admin/products", ProductWithPrice("Категория", 1, 99, 1))).Status);
    }

    [Fact]
    public async Task Admin_ManagesCategories_DuplicateIsConflict()
    {
        var admin = await Admin();

        var created = (await admin.Post("api/admin/categories", new CategoryRequest("Игрушки"))).Json<CategoryDto>();
        var duplicate = await admin.Post("api/admin/categories", new CategoryRequest("Игрушки"));
        var renamed = (await admin.Put($"api/admin/categories/{created.Id}", new CategoryRequest("Игры"))).Json<CategoryDto>();

        Assert.Equal(HttpStatusCode.Conflict, duplicate.Status);
        Assert.Equal("Игры", renamed.Name);
    }

    [Fact]
    public async Task Admin_ManagesPickupPoints_UsedPointCannotBeDeleted()
    {
        var admin = await Admin();
        var user = await User();
        var created = (await admin.Post("api/admin/pickup-points", new PickupPointRequest("ПВЗ Новый", "Москва, Арбат, 1"))).Json<PickupPointDto>();
        await user.Post("api/orders", new CreateOrderRequest(DeliveryMethods.Pickup, null, created.Id, new OrderItemRequest(Cable, 1)));

        var used = await admin.Delete($"api/admin/pickup-points/{created.Id}");
        var free = await admin.Delete("api/admin/pickup-points/3");

        Assert.Equal(HttpStatusCode.Conflict, used.Status);
        Assert.Equal(HttpStatusCode.NoContent, free.Status);
    }

    [Fact]
    public async Task Admin_ChangesOrderStatus_AndSeesAllOrders()
    {
        var admin = await Admin();
        var user = await User();
        var order = (await user.Post("api/orders", PickupOrder(Cable))).Json<OrderDto>();

        var updated = (await admin.Put($"api/admin/orders/{order.Id}/status", new StatusRequest(OrderStatuses.Processing))).Json<OrderDto>();
        var all = (await admin.Get("api/admin/orders")).Json<List<OrderSummaryDto>>();

        Assert.Equal(OrderStatuses.Processing, updated.Status);
        Assert.Contains(all, o => o.Id == order.Id && o.UserLogin == "buyer");
        Assert.Equal(HttpStatusCode.BadRequest,
            (await admin.Put($"api/admin/orders/{order.Id}/status", new StatusRequest("Выдумка"))).Status);
    }

    [Fact]
    public async Task Admin_DeletesIncorrectReview()
    {
        var admin = await Admin();
        var user = await User();
        var review = (await user.Post($"api/products/{Mug}/reviews", new ReviewRequest(1, "Реклама"))).Json<ReviewDto>();

        var deleted = await admin.Delete($"api/admin/reviews/{review.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleted.Status);
        Assert.Empty((await Guest().Get($"api/products/{Mug}/reviews")).Json<List<ReviewDto>>());
    }
}
