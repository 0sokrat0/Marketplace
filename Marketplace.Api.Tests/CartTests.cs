using System.Net;
using Marketplace.Contracts;

namespace Marketplace.Api.Tests;

public class CartTests : ApiTestBase
{
    [Theory]
    [InlineData(OutOfStock)]
    [InlineData(Withdrawn)]
    public async Task UnavailableProduct_CannotBeAdded(int productId)
    {
        var user = await User();

        var result = await user.Post("api/cart/items", ToCart(productId));

        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
    }

    [Fact]
    public async Task Quantity_CannotExceedStock()
    {
        var user = await User();
        await user.Post("api/cart/items", ToCart(Speaker));

        var result = await user.Put($"api/cart/items/{Speaker}", new QuantityRequest(9));

        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
        Assert.Contains("осталось 8", result.Error);
    }

    [Fact]
    public async Task AddChangeAndRemove_KeepTotalsConsistent()
    {
        var user = await User();

        await user.Post("api/cart/items", ToCart(Cable, 2));
        var afterAdd = (await user.Post("api/cart/items", ToCart(Mug))).Json<CartDto>();
        var afterChange = (await user.Put($"api/cart/items/{Cable}", new QuantityRequest(3))).Json<CartDto>();
        var afterRemove = (await user.Delete($"api/cart/items/{Mug}")).Json<CartDto>();

        Assert.Equal(2 * 310m + 350m, afterAdd.Total);
        Assert.Equal(3 * 310m + 350m, afterChange.Total);
        Assert.Single(afterRemove.Items);
        Assert.Equal(3 * 310m, afterRemove.Total);
    }

    [Fact]
    public async Task Favorites_CanBeMovedToCart()
    {
        var user = await User();
        await user.Post($"api/favorites/{Mug}");

        var cart = (await user.Post($"api/favorites/{Mug}/to-cart")).Json<CartDto>();
        var favorites = (await user.Get("api/favorites")).Json<List<ProductDto>>();

        Assert.Single(cart.Items);
        Assert.Empty(favorites);
    }

    [Fact]
    public async Task Cart_IsPrivateToEachUser()
    {
        var first = await User("first");
        var second = await User("second");
        await first.Post("api/cart/items", ToCart(Mug));

        var other = (await second.Get("api/cart")).Json<CartDto>();

        Assert.Empty(other.Items);
    }
}
