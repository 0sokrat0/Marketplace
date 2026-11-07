using Marketplace.Api.Infrastructure;
using Marketplace.Api.Services.Favorites;
using Marketplace.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/favorites")]
public class FavoritesController(IFavoriteService favorites) : ControllerBase
{
    [HttpGet]
    public Task<List<ProductDto>> Get() => favorites.GetAsync(User.GetUserId());

    [HttpPost("{productId:int}")]
    public async Task<IActionResult> Add(int productId)
    {
        await favorites.AddAsync(User.GetUserId(), productId);
        return NoContent();
    }

    [HttpDelete("{productId:int}")]
    public async Task<IActionResult> Remove(int productId)
    {
        await favorites.RemoveAsync(User.GetUserId(), productId);
        return NoContent();
    }

    [HttpPost("{productId:int}/to-cart")]
    public Task<CartDto> MoveToCart(int productId) => favorites.MoveToCartAsync(User.GetUserId(), productId);
}
