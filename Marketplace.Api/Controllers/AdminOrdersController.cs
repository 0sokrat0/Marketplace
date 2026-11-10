using Marketplace.Api.Services.Orders;
using Marketplace.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Api.Controllers;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin/orders")]
public class AdminOrdersController(IOrderService orders) : ControllerBase
{
    [HttpGet]
    public Task<List<OrderSummaryDto>> List() => orders.ListAllAsync();

    [HttpGet("{id:int}")]
    public Task<OrderDto> Get(int id) => orders.GetAnyAsync(id);

    [HttpPut("{id:int}/status")]
    public Task<OrderDto> SetStatus(int id, StatusRequest request) => orders.SetStatusAsync(id, request.Status);
}
