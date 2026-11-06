using Marketplace.Contracts;

namespace Marketplace.Api.Services.Cart;

public interface ICartService
{
    Task<CartDto> GetAsync(int userId);

    Task<CartDto> AddAsync(int userId, AddToCartRequest request);

    Task<CartDto> SetQuantityAsync(int userId, int productId, int quantity);

    Task<CartDto> RemoveAsync(int userId, int productId);
}
