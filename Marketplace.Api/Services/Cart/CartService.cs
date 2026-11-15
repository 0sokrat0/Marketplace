using Marketplace.Api.Entities;
using Marketplace.Api.Infrastructure;
using Marketplace.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Api.Services.Cart;

public class CartService(MarketplaceDbContext db) : ICartService
{
    public async Task<Result<CartDto>> GetAsync(int userId) => await LoadAsync(userId);

    public async Task<Result<CartDto>> AddAsync(int userId, AddToCartRequest request)
    {
        if (request.Quantity < 1)
        {
            return Result.InvalidData("Количество должно быть больше нуля");
        }

        var product = await db.Products.FindAsync(request.ProductId);
        if (product is null)
        {
            return Result.NotFound("Товар не найден");
        }

        var item = await db.CartItems.FindAsync(userId, request.ProductId);
        var quantity = request.Quantity + (item?.Quantity ?? 0);
        if (CheckCanBuy(product, quantity) is { } unavailable)
        {
            return unavailable;
        }

        if (item is null)
        {
            db.CartItems.Add(new CartItem { UserId = userId, ProductId = product.Id, Quantity = quantity });
        }
        else
        {
            item.Quantity = quantity;
        }

        await db.SaveChangesAsync();
        return await LoadAsync(userId);
    }

    public async Task<Result<CartDto>> SetQuantityAsync(int userId, int productId, int quantity)
    {
        if (quantity < 1)
        {
            return Result.InvalidData("Количество должно быть больше нуля, для удаления уберите товар из корзины");
        }

        var item = await db.CartItems.FindAsync(userId, productId);
        if (item is null)
        {
            return Result.NotFound("Товара нет в корзине");
        }

        var product = await db.Products.FindAsync(productId);
        if (product is null)
        {
            return Result.NotFound("Товар не найден");
        }

        if (CheckCanBuy(product, quantity) is { } unavailable)
        {
            return unavailable;
        }

        item.Quantity = quantity;
        await db.SaveChangesAsync();
        return await LoadAsync(userId);
    }

    public async Task<Result<CartDto>> RemoveAsync(int userId, int productId)
    {
        var item = await db.CartItems.FindAsync(userId, productId);
        if (item is not null)
        {
            db.CartItems.Remove(item);
            await db.SaveChangesAsync();
        }

        return await LoadAsync(userId);
    }

    private async Task<CartDto> LoadAsync(int userId)
    {
        var items = await db.CartItems
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .Include(c => c.Product)
            .OrderBy(c => c.Product.Name)
            .ToListAsync();

        var dtos = items.Select(c => new CartItemDto(
            c.ProductId,
            c.Product.Name,
            c.Product.ImageFileName,
            c.Product.Price,
            c.Quantity,
            c.Product.Stock,
            c.Product.IsAvailable && c.Product.Stock > 0,
            c.Product.Price * c.Quantity)).ToList();

        return new CartDto(dtos, dtos.Sum(i => i.LineTotal));
    }

    private static Failure? CheckCanBuy(Product product, int quantity)
    {
        if (!product.IsAvailable || product.Stock <= 0)
        {
            return Result.InvalidData($"Товар «{product.Name}» недоступен для покупки");
        }

        if (quantity > product.Stock)
        {
            return Result.InvalidData($"Товара «{product.Name}» на складе осталось {product.Stock} шт.");
        }

        return null;
    }
}
