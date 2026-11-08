using Marketplace.Contracts;

namespace Marketplace.Api.Services.Orders;

public interface IOrderService
{
    Task<OrderDto> CreateAsync(int userId, CreateOrderRequest request);

    Task<OrderDto> GetForUserAsync(int userId, int orderId);
}
