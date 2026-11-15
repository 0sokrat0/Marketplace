using Marketplace.Api.Infrastructure;
using Marketplace.Contracts;

namespace Marketplace.Api.Services.Cart;

public interface ICartService
{
    Task<Result<CartDto>> GetAsync(int userId);

    Task<Result<CartDto>> AddAsync(int userId, AddToCartRequest request);

    Task<Result<CartDto>> SetQuantityAsync(int userId, int productId, int quantity);

    Task<Result<CartDto>> RemoveAsync(int userId, int productId);
}
