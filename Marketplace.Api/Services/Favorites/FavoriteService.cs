using Marketplace.Api.Entities;
using Marketplace.Api.Infrastructure;
using Marketplace.Api.Services.Cart;
using Marketplace.Api.Services.Catalog;
using Marketplace.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Api.Services.Favorites;

public class FavoriteService(MarketplaceDbContext db, ICatalogService catalog, ICartService cart) : IFavoriteService
{
    public async Task<Result<List<ProductDto>>> GetAsync(int userId)
    {
        var ids = await db.Favorites.Where(f => f.UserId == userId).Select(f => f.ProductId).ToListAsync();
        return await catalog.GetByIdsAsync(ids);
    }

    public async Task<Result> AddAsync(int userId, int productId)
    {
        if (!await db.Products.AnyAsync(p => p.Id == productId))
        {
            return Result.NotFound("Товар не найден");
        }

        if (await db.Favorites.FindAsync(userId, productId) is null)
        {
            db.Favorites.Add(new FavoriteProduct { UserId = userId, ProductId = productId });
            await db.SaveChangesAsync();
        }

        return Result.NoContent();
    }

    public async Task<Result> RemoveAsync(int userId, int productId)
    {
        var favorite = await db.Favorites.FindAsync(userId, productId);
        if (favorite is not null)
        {
            db.Favorites.Remove(favorite);
            await db.SaveChangesAsync();
        }

        return Result.NoContent();
    }

    public async Task<Result<CartDto>> MoveToCartAsync(int userId, int productId)
    {
        var favorite = await db.Favorites.FindAsync(userId, productId);
        if (favorite is null)
        {
            return Result.NotFound("Товара нет в избранном");
        }

        var added = await cart.AddAsync(userId, new AddToCartRequest(productId, 1));
        if (!added.IsSuccess)
        {
            return added.ToFailure();
        }

        db.Favorites.Remove(favorite);
        await db.SaveChangesAsync();
        return added;
    }
}
