using Marketplace.Api.Infrastructure;
using Marketplace.Contracts;

namespace Marketplace.Api.Services.Favorites;

public interface IFavoriteService
{
    Task<Result<List<ProductDto>>> GetAsync(int userId);

    Task<Result> AddAsync(int userId, int productId);

    Task<Result> RemoveAsync(int userId, int productId);

    Task<Result<CartDto>> MoveToCartAsync(int userId, int productId);
}
