using Marketplace.Contracts;

namespace Marketplace.Api.Services.Favorites;

public interface IFavoriteService
{
    Task<List<ProductDto>> GetAsync(int userId);

    Task AddAsync(int userId, int productId);

    Task RemoveAsync(int userId, int productId);

    Task<CartDto> MoveToCartAsync(int userId, int productId);
}
