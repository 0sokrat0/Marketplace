using Marketplace.Api.Entities;
using Marketplace.Api.Infrastructure;
using Marketplace.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Api.Services.Cart;

public class CartService(MarketplaceDbContext db) : ICartService
{
    public async Task<CartDto> GetAsync(int userId)
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

    public async Task<CartDto> AddAsync(int userId, AddToCartRequest request)
    {
        if (request.Quantity < 1)
        {
            throw AppException.BadRequest("Количество должно быть больше нуля");
        }

        var product = await FindProduct(request.ProductId);
        EnsureCanBuy(product);

        var item = await db.CartItems.FindAsync(userId, request.ProductId);
        var quantity = request.Quantity + (item?.Quantity ?? 0);
        EnsureEnoughStock(product, quantity);

        if (item is null)
        {
            db.CartItems.Add(new CartItem { UserId = userId, ProductId = product.Id, Quantity = quantity });
        }
        else
        {
            item.Quantity = quantity;
        }

        await db.SaveChangesAsync();
        return await GetAsync(userId);
    }

    public async Task<CartDto> SetQuantityAsync(int userId, int productId, int quantity)
    {
        if (quantity < 1)
        {
            throw AppException.BadRequest("Количество должно быть больше нуля, для удаления уберите товар из корзины");
        }

        var item = await db.CartItems.FindAsync(userId, productId)
            ?? throw AppException.NotFound("Товара нет в корзине");
        var product = await FindProduct(productId);
        EnsureCanBuy(product);
        EnsureEnoughStock(product, quantity);

        item.Quantity = quantity;
        await db.SaveChangesAsync();
        return await GetAsync(userId);
    }

    public async Task<CartDto> RemoveAsync(int userId, int productId)
    {
        var item = await db.CartItems.FindAsync(userId, productId);
        if (item is not null)
        {
            db.CartItems.Remove(item);
            await db.SaveChangesAsync();
        }

        return await GetAsync(userId);
    }

    private async Task<Product> FindProduct(int productId) =>
        await db.Products.FindAsync(productId) ?? throw AppException.NotFound("Товар не найден");

    private static void EnsureCanBuy(Product product)
    {
        if (!product.IsAvailable || product.Stock <= 0)
        {
            throw AppException.BadRequest($"Товар «{product.Name}» недоступен для покупки");
        }
    }

    private static void EnsureEnoughStock(Product product, int quantity)
    {
        if (quantity > product.Stock)
        {
            throw AppException.BadRequest($"Товара «{product.Name}» на складе осталось {product.Stock} шт.");
        }
    }
}
