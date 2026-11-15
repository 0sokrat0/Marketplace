using Marketplace.Api.Entities;
using Marketplace.Api.Infrastructure;
using Marketplace.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Api.Services.Catalog;

public class CatalogService(MarketplaceDbContext db) : ICatalogService
{
    public async Task<Result<List<CategoryDto>>> GetCategoriesAsync() =>
        await db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name))
            .ToListAsync();

    public async Task<Result<List<PickupPointDto>>> GetPickupPointsAsync() =>
        await db.PickupPoints
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new PickupPointDto(p.Id, p.Name, p.Address))
            .ToListAsync();

    public async Task<Result<List<ProductDto>>> GetProductsAsync(ProductFilter filter)
    {
        var query = db.Products.AsNoTracking().Include(p => p.Category).AsQueryable();

        if (filter.CategoryId is { } categoryId)
        {
            query = query.Where(p => p.CategoryId == categoryId);
        }

        if (filter.MinPrice is { } minPrice)
        {
            query = query.Where(p => p.Price >= minPrice);
        }

        if (filter.MaxPrice is { } maxPrice)
        {
            query = query.Where(p => p.Price <= maxPrice);
        }

        var products = await query.OrderBy(p => p.Name).ToListAsync();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            products = products.Where(p => p.Name.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return await ToDtosAsync(products);
    }

    public async Task<Result<ProductDto>> GetProductAsync(int id)
    {
        var product = await db.Products.AsNoTracking().Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
        {
            return Result.NotFound("Товар не найден");
        }

        return (await ToDtosAsync([product]))[0];
    }

    public async Task<List<ProductDto>> GetByIdsAsync(IReadOnlyCollection<int> ids)
    {
        var products = await db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => ids.Contains(p.Id))
            .OrderBy(p => p.Name)
            .ToListAsync();
        return await ToDtosAsync(products);
    }

    private async Task<List<ProductDto>> ToDtosAsync(List<Product> products)
    {
        var ids = products.Select(p => p.Id).ToList();
        var ratings = await db.Reviews
            .Where(r => ids.Contains(r.ProductId))
            .GroupBy(r => r.ProductId)
            .Select(g => new { ProductId = g.Key, Average = g.Average(r => r.Rating), Count = g.Count() })
            .ToDictionaryAsync(x => x.ProductId);

        return products.Select(p =>
        {
            var rating = ratings.GetValueOrDefault(p.Id);
            return new ProductDto(
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.ImageFileName,
                p.CategoryId,
                p.Category.Name,
                p.Stock,
                p.PackSize,
                p.IsAvailable,
                p.IsAvailable && p.Stock > 0,
                rating is null ? 0 : Math.Round(rating.Average, 1),
                rating?.Count ?? 0);
        }).ToList();
    }
}
