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

        var result = await user.Post("api/Cart/Items", ToCart(productId));

        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
    }

    [Fact]
    public async Task Quantity_CannotExceedStock()
    {
        var user = await User();
        await user.Post("api/Cart/Items", ToCart(Speaker));

        var result = await user.Put($"api/Cart/Items/{Speaker}", new QuantityRequest(9));

        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
        Assert.Contains("осталось 8", result.Error);
    }

    [Fact]
    public async Task AddChangeAndRemove_KeepTotalsConsistent()
    {
        var user = await User();

        await user.Post("api/Cart/Items", ToCart(Cable, 2));
        var afterAdd = (await user.Post("api/Cart/Items", ToCart(Mug))).Json<CartDto>();
        var afterChange = (await user.Put($"api/Cart/Items/{Cable}", new QuantityRequest(3))).Json<CartDto>();
        var afterRemove = (await user.Delete($"api/Cart/Items/{Mug}")).Json<CartDto>();

        Assert.Equal(2 * 310m + 350m, afterAdd.Total);
        Assert.Equal(3 * 310m + 350m, afterChange.Total);
        Assert.Single(afterRemove.Items);
        Assert.Equal(3 * 310m, afterRemove.Total);
    }

    [Fact]
    public async Task Favorites_CanBeMovedToCart()
    {
        var user = await User();
        await user.Post($"api/Favorite/{Mug}");

        var cart = (await user.Post($"api/Favorite/{Mug}/ToCart")).Json<CartDto>();
        var favorites = (await user.Get("api/Favorite")).Json<List<ProductDto>>();

        Assert.Single(cart.Items);
        Assert.Empty(favorites);
    }

    [Fact]
    public async Task Cart_IsPrivateToEachUser()
    {
        var first = await User("first");
        var second = await User("second");
        await first.Post("api/Cart/Items", ToCart(Mug));

        var other = (await second.Get("api/Cart")).Json<CartDto>();

        Assert.Empty(other.Items);
    }
}
