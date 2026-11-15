using Marketplace.Api.Infrastructure;
using Marketplace.Api.Services.Favorites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FavoriteController(IFavoriteService favorites) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() => (await favorites.GetAsync(User.GetUserId())).ToActionResult();

    [HttpPost("{productId:int}")]
    public async Task<IActionResult> Add([FromRoute] int productId) =>
        (await favorites.AddAsync(User.GetUserId(), productId)).ToActionResult();

    [HttpDelete("{productId:int}")]
    public async Task<IActionResult> Remove([FromRoute] int productId) =>
        (await favorites.RemoveAsync(User.GetUserId(), productId)).ToActionResult();

    [HttpPost("{productId:int}/ToCart")]
    public async Task<IActionResult> MoveToCart([FromRoute] int productId) =>
        (await favorites.MoveToCartAsync(User.GetUserId(), productId)).ToActionResult();
}
