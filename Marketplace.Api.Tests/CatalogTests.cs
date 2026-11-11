using System.Net;
using Marketplace.Contracts;

namespace Marketplace.Api.Tests;

public class CatalogTests : ApiTestBase
{
    [Fact]
    public async Task Guest_CanBrowseCatalogAndReadReviews()
    {
        var guest = Guest();

        var products = await guest.Get("api/products");
        var reviews = await guest.Get($"api/products/{Mug}/reviews");

        Assert.Equal(HttpStatusCode.OK, products.Status);
        Assert.Equal(13, products.Json<List<ProductDto>>().Count);
        Assert.Equal(HttpStatusCode.OK, reviews.Status);
    }

    [Fact]
    public async Task Guest_CannotUseCartFavoritesOrOrders()
    {
        var guest = Guest();

        Assert.Equal(HttpStatusCode.Unauthorized, (await guest.Get("api/cart")).Status);
        Assert.Equal(HttpStatusCode.Unauthorized, (await guest.Get("api/favorites")).Status);
        Assert.Equal(HttpStatusCode.Unauthorized, (await guest.Get("api/orders")).Status);
    }

    [Fact]
    public async Task Search_IgnoresCaseForCyrillic()
    {
        var found = (await Guest().Get("api/products?search=" + Uri.EscapeDataString("КРУЖ"))).Json<List<ProductDto>>();

        Assert.Single(found);
        Assert.Equal("Кружка керамическая", found[0].Name);
    }

    [Fact]
    public async Task Filter_ByCategoryAndPrice()
    {
        var guest = Guest();

        var electronics = (await guest.Get("api/products?categoryId=3")).Json<List<ProductDto>>();
        var middle = (await guest.Get("api/products?minPrice=500&maxPrice=1000")).Json<List<ProductDto>>();

        Assert.Equal(3, electronics.Count);
        Assert.All(electronics, p => Assert.Equal(3, p.CategoryId));
        Assert.Equal(3, middle.Count);
        Assert.All(middle, p => Assert.InRange(p.Price, 500m, 1000m));
    }

    [Fact]
    public async Task Product_ExposesPackSizeAndAvailability()
    {
        var guest = Guest();

        var pens = (await guest.Get("api/products/1")).Json<ProductDto>();
        var empty = (await guest.Get($"api/products/{OutOfStock}")).Json<ProductDto>();
        var withdrawn = (await guest.Get($"api/products/{Withdrawn}")).Json<ProductDto>();

        Assert.Equal(10, pens.PackSize);
        Assert.True(pens.CanBuy);
        Assert.False(empty.CanBuy);
        Assert.False(withdrawn.CanBuy);
        Assert.False(withdrawn.IsAvailable);
    }
}
