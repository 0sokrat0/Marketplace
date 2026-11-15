using Marketplace.Api.Infrastructure;
using Marketplace.Api.Services.Cart;
using Marketplace.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController(ICartService cart) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() => (await cart.GetAsync(User.GetUserId())).ToActionResult();

    [HttpPost("Items")]
    public async Task<IActionResult> Add([FromBody] AddToCartRequest request) =>
        (await cart.AddAsync(User.GetUserId(), request)).ToActionResult();

    [HttpPut("Items/{productId:int}")]
    public async Task<IActionResult> SetQuantity([FromRoute] int productId, [FromBody] QuantityRequest request) =>
        (await cart.SetQuantityAsync(User.GetUserId(), productId, request.Quantity)).ToActionResult();

    [HttpDelete("Items/{productId:int}")]
    public async Task<IActionResult> Remove([FromRoute] int productId) =>
        (await cart.RemoveAsync(User.GetUserId(), productId)).ToActionResult();
}
