using Marketplace.Api.Infrastructure;
using Marketplace.Api.Services.Orders;
using Marketplace.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController(IOrderService orders) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetAll() => (await orders.ListAllAsync()).ToActionResult();

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetById([FromRoute] int id) => (await orders.GetAnyAsync(id)).ToActionResult();

    [HttpGet("My")]
    public async Task<IActionResult> GetMy([FromQuery] string? sort) =>
        (await orders.ListForUserAsync(User.GetUserId(), sort != "old")).ToActionResult();

    [HttpGet("My/{id:int}")]
    public async Task<IActionResult> GetMyById([FromRoute] int id) =>
        (await orders.GetForUserAsync(User.GetUserId(), id)).ToActionResult();

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request) =>
        (await orders.CreateAsync(User.GetUserId(), request)).ToActionResult();

    [HttpPut("{id:int}/Status")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> SetStatus([FromRoute] int id, [FromBody] StatusRequest request) =>
        (await orders.SetStatusAsync(id, request.Status)).ToActionResult();
}
