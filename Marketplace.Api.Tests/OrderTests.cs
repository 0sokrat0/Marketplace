using System.Net;
using Marketplace.Contracts;

namespace Marketplace.Api.Tests;

public class OrderTests : ApiTestBase
{
    private static CreateOrderRequest Delivery(int productId, int quantity = 1) =>
        new(DeliveryMethods.Delivery, "Москва, ул. Пушкина, 1", null, new OrderItemRequest(productId, quantity));

    [Fact]
    public async Task DeliveryBelowThreshold_IsRejected_PickupIsAccepted()
    {
        var user = await User();

        var delivery = await user.Post("api/orders", Delivery(Cable));
        var pickup = await user.Post("api/orders", PickupOrder(Cable));

        Assert.Equal(HttpStatusCode.BadRequest, delivery.Status);
        Assert.Equal(HttpStatusCode.OK, pickup.Status);
    }

    [Fact]
    public async Task DeliveryAtOrAboveThreshold_IsAccepted()
    {
        var user = await User();

        var result = await user.Post("api/orders", Delivery(Speaker));

        Assert.Equal(HttpStatusCode.OK, result.Status);
        var order = result.Json<OrderDto>();
        Assert.Equal(DeliveryMethods.Delivery, order.DeliveryMethod);
        Assert.Equal("Москва, ул. Пушкина, 1", order.Destination);
    }

    [Fact]
    public async Task DeliveryRequiresAddress_PickupRequiresExistingPoint()
    {
        var user = await User();

        var noAddress = await user.Post("api/orders",
            new CreateOrderRequest(DeliveryMethods.Delivery, null, null, new OrderItemRequest(Speaker, 1)));
        var noPoint = await user.Post("api/orders",
            new CreateOrderRequest(DeliveryMethods.Pickup, null, 99, new OrderItemRequest(Cable, 1)));

        Assert.Equal(HttpStatusCode.BadRequest, noAddress.Status);
        Assert.Equal(HttpStatusCode.BadRequest, noPoint.Status);
    }

    [Fact]
    public async Task CartCheckout_DecreasesStockAndClearsCart()
    {
        var user = await User();
        await user.Post("api/cart/items", ToCart(Cable, 2));

        var order = (await user.Post("api/orders", new CreateOrderRequest(DeliveryMethods.Pickup, null, 1, null))).Json<OrderDto>();

        Assert.Equal(620m, order.Total);
        Assert.Equal(OrderStatuses.Placed, order.Status);
        Assert.Empty((await user.Get("api/cart")).Json<CartDto>().Items);
        Assert.Equal(98, (await Guest().Get($"api/products/{Cable}")).Json<ProductDto>().Stock);
    }

    [Fact]
    public async Task BuyNow_DoesNotTouchCart()
    {
        var user = await User();
        await user.Post("api/cart/items", ToCart(Mug));

        await user.Post("api/orders", PickupOrder(Cable));

        Assert.Single((await user.Get("api/cart")).Json<CartDto>().Items);
    }

    [Fact]
    public async Task OutOfStockOrWithdrawn_CannotBeOrdered()
    {
        var user = await User();

        Assert.Equal(HttpStatusCode.BadRequest, (await user.Post("api/orders", PickupOrder(OutOfStock))).Status);
        Assert.Equal(HttpStatusCode.BadRequest, (await user.Post("api/orders", PickupOrder(Withdrawn))).Status);
        Assert.Equal(HttpStatusCode.BadRequest, (await user.Post("api/orders", PickupOrder(Speaker, 99))).Status);
    }

    [Fact]
    public async Task EmptyCart_CannotBeOrdered()
    {
        var user = await User();

        var result = await user.Post("api/orders", new CreateOrderRequest(DeliveryMethods.Pickup, null, 1, null));

        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
    }

    [Fact]
    public async Task Order_KeepsPriceAtPurchase_WhenProductPriceChanges()
    {
        var user = await User();
        var admin = await Admin();
        var placed = (await user.Post("api/orders", PickupOrder(Speaker, 2))).Json<OrderDto>();

        await admin.Put($"api/admin/products/{Speaker}", ProductWithPrice("Портативная колонка", 3000, 3, 6));

        var old = (await user.Get($"api/orders/{placed.Id}")).Json<OrderDto>();
        var fresh = (await user.Post("api/orders", PickupOrder(Speaker))).Json<OrderDto>();
        Assert.Equal(4980m, old.Total);
        Assert.Equal(2490m, old.Items[0].UnitPrice);
        Assert.Equal(3000m, fresh.Total);
    }

    [Fact]
    public async Task History_SortsByDate_AndHidesOtherUsersOrders()
    {
        var user = await User("owner");
        var other = await User("stranger");
        var first = (await user.Post("api/orders", PickupOrder(Mug))).Json<OrderDto>();
        await Task.Delay(1100);
        var second = (await user.Post("api/orders", PickupOrder(Cable))).Json<OrderDto>();
        var foreign = (await other.Post("api/orders", PickupOrder(Cable))).Json<OrderDto>();

        var newest = (await user.Get("api/orders?sort=new")).Json<List<OrderSummaryDto>>();
        var oldest = (await user.Get("api/orders?sort=old")).Json<List<OrderSummaryDto>>();

        Assert.Equal([second.Id, first.Id], newest.Select(o => o.Id));
        Assert.Equal([first.Id, second.Id], oldest.Select(o => o.Id));
        Assert.Equal(HttpStatusCode.NotFound, (await user.Get($"api/orders/{foreign.Id}")).Status);
    }

    [Fact]
    public async Task CancellingAnOrder_ReturnsStock_AndFreezesStatus()
    {
        var user = await User();
        var admin = await Admin();
        var order = (await user.Post("api/orders", PickupOrder(Speaker, 3))).Json<OrderDto>();
        Assert.Equal(5, (await Guest().Get($"api/products/{Speaker}")).Json<ProductDto>().Stock);

        var cancelled = await admin.Put($"api/admin/orders/{order.Id}/status", new StatusRequest(OrderStatuses.Cancelled));

        Assert.Equal(OrderStatuses.Cancelled, cancelled.Json<OrderDto>().Status);
        Assert.Equal(8, (await Guest().Get($"api/products/{Speaker}")).Json<ProductDto>().Stock);
        var again = await admin.Put($"api/admin/orders/{order.Id}/status", new StatusRequest(OrderStatuses.Received));
        Assert.Equal(HttpStatusCode.BadRequest, again.Status);
    }
}
