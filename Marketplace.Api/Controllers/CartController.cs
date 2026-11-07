using Marketplace.Api.Infrastructure;
using Marketplace.Api.Services.Cart;
using Marketplace.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/cart")]
public class CartController(ICartService cart) : ControllerBase
{
    [HttpGet]
    public Task<CartDto> Get() => cart.GetAsync(User.GetUserId());

    [HttpPost("items")]
    public Task<CartDto> Add(AddToCartRequest request) => cart.AddAsync(User.GetUserId(), request);

    [HttpPut("items/{productId:int}")]
    public Task<CartDto> SetQuantity(int productId, QuantityRequest request) =>
        cart.SetQuantityAsync(User.GetUserId(), productId, request.Quantity);

    [HttpDelete("items/{productId:int}")]
    public Task<CartDto> Remove(int productId) => cart.RemoveAsync(User.GetUserId(), productId);
}
