using Marketplace.Api.Infrastructure;
using Marketplace.Api.Services.Orders;
using Marketplace.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
public class OrdersController(IOrderService orders) : ControllerBase
{
    [HttpPost]
    public Task<OrderDto> Create(CreateOrderRequest request) => orders.CreateAsync(User.GetUserId(), request);

    [HttpGet]
    public Task<List<OrderSummaryDto>> List([FromQuery] string? sort) =>
        orders.ListForUserAsync(User.GetUserId(), sort != "old");

    [HttpGet("{id:int}")]
    public Task<OrderDto> Get(int id) => orders.GetForUserAsync(User.GetUserId(), id);
}
