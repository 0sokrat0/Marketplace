using Marketplace.Contracts;

namespace Marketplace.Api.Services.Orders;

public interface IOrderService
{
    Task<OrderDto> CreateAsync(int userId, CreateOrderRequest request);

    Task<List<OrderSummaryDto>> ListForUserAsync(int userId, bool newestFirst);

    Task<OrderDto> GetForUserAsync(int userId, int orderId);

    Task<List<OrderSummaryDto>> ListAllAsync();

    Task<OrderDto> GetAnyAsync(int orderId);

    Task<OrderDto> SetStatusAsync(int orderId, string status);
}
