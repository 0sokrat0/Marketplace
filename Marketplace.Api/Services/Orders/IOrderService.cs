using Marketplace.Api.Infrastructure;
using Marketplace.Contracts;

namespace Marketplace.Api.Services.Orders;

public interface IOrderService
{
    Task<Result<OrderDto>> CreateAsync(int userId, CreateOrderRequest request);

    Task<Result<List<OrderSummaryDto>>> ListForUserAsync(int userId, bool newestFirst);

    Task<Result<OrderDto>> GetForUserAsync(int userId, int orderId);

    Task<Result<List<OrderSummaryDto>>> ListAllAsync();

    Task<Result<OrderDto>> GetAnyAsync(int orderId);

    Task<Result<OrderDto>> SetStatusAsync(int orderId, string status);
}
