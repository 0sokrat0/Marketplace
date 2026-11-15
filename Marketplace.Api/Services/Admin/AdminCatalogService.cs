using Marketplace.Api.Entities;
using Marketplace.Api.Infrastructure;
using Marketplace.Api.Services.Catalog;
using Marketplace.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Api.Services.Admin;

public class AdminCatalogService(MarketplaceDbContext db, ICatalogService catalog) : IAdminCatalogService
{
    private const int MaxNameLength = 200;
    private const int MaxDescriptionLength = 2000;
    private const int MaxAddressLength = 300;

    public async Task<Result<ProductDto>> CreateProductAsync(ProductRequest request)
    {
        if (await CheckProduct(request) is { } invalid)
        {
            return invalid;
        }

        var product = new Product();
        Apply(product, request);
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return await catalog.GetProductAsync(product.Id);
    }

    public async Task<Result<ProductDto>> UpdateProductAsync(int id, ProductRequest request)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null)
        {
            return Result.NotFound("Товар не найден");
        }

        if (await CheckProduct(request) is { } invalid)
        {
            return invalid;
        }

        Apply(product, request);
        await db.SaveChangesAsync();
        return await catalog.GetProductAsync(product.Id);
    }

    public async Task<Result<CategoryDto>> CreateCategoryAsync(CategoryRequest request)
    {
        if (CheckName(request.Name) is { } invalid)
        {
            return invalid;
        }

        var name = request.Name.Trim();
        if (await CategoryNameTaken(name, null))
        {
            return Result.Conflict("Категория с таким названием уже есть");
        }

        var category = new Category { Name = name };
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return new CategoryDto(category.Id, category.Name);
    }

    public async Task<Result<CategoryDto>> UpdateCategoryAsync(int id, CategoryRequest request)
    {
        var category = await db.Categories.FindAsync(id);
        if (category is null)
        {
            return Result.NotFound("Категория не найдена");
        }

        if (CheckName(request.Name) is { } invalid)
        {
            return invalid;
        }

        var name = request.Name.Trim();
        if (await CategoryNameTaken(name, id))
        {
            return Result.Conflict("Категория с таким названием уже есть");
        }

        category.Name = name;
        await db.SaveChangesAsync();
        return new CategoryDto(category.Id, category.Name);
    }

    public async Task<Result<PickupPointDto>> CreatePickupPointAsync(PickupPointRequest request)
    {
        if (CheckPickupPoint(request) is { } invalid)
        {
            return invalid;
        }

        var point = new PickupPoint();
        ApplyPickupPoint(point, request);
        db.PickupPoints.Add(point);
        await db.SaveChangesAsync();
        return new PickupPointDto(point.Id, point.Name, point.Address);
    }

    public async Task<Result<PickupPointDto>> UpdatePickupPointAsync(int id, PickupPointRequest request)
    {
        var point = await db.PickupPoints.FindAsync(id);
        if (point is null)
        {
            return Result.NotFound("Пункт выдачи не найден");
        }

        if (CheckPickupPoint(request) is { } invalid)
        {
            return invalid;
        }

        ApplyPickupPoint(point, request);
        await db.SaveChangesAsync();
        return new PickupPointDto(point.Id, point.Name, point.Address);
    }

    public async Task<Result> DeletePickupPointAsync(int id)
    {
        var point = await db.PickupPoints.FindAsync(id);
        if (point is null)
        {
            return Result.NotFound("Пункт выдачи не найден");
        }

        if (await db.Orders.AnyAsync(o => o.PickupPointId == id))
        {
            return Result.Conflict("Пункт выдачи используется в заказах, удалить его нельзя");
        }

        db.PickupPoints.Remove(point);
        await db.SaveChangesAsync();
        return Result.NoContent();
    }

    private async Task<Failure?> CheckProduct(ProductRequest request)
    {
        if (CheckName(request.Name) is { } invalidName)
        {
            return invalidName;
        }

        if (request.Description?.Length > MaxDescriptionLength)
        {
            return Result.InvalidData($"Описание не длиннее {MaxDescriptionLength} символов");
        }

        if (request.Price < 0)
        {
            return Result.InvalidData("Цена не может быть отрицательной");
        }

        if (request.Stock < 0)
        {
            return Result.InvalidData("Остаток на складе не может быть отрицательным");
        }

        if (request.PackSize is <= 0)
        {
            return Result.InvalidData("Количество штук в упаковке должно быть больше нуля");
        }

        if (!await db.Categories.AnyAsync(c => c.Id == request.CategoryId))
        {
            return Result.InvalidData("Выберите существующую категорию");
        }

        return null;
    }

    private static void Apply(Product product, ProductRequest request)
    {
        product.Name = request.Name.Trim();
        product.Description = request.Description?.Trim() ?? string.Empty;
        product.Price = Math.Round(request.Price, 2);
        product.ImageFileName = Path.GetFileName(request.ImageFileName?.Trim() ?? string.Empty);
        product.CategoryId = request.CategoryId;
        product.Stock = request.Stock;
        product.PackSize = request.PackSize;
        product.IsAvailable = request.IsAvailable;
    }

    private static Failure? CheckPickupPoint(PickupPointRequest request)
    {
        if (CheckName(request.Name) is { } invalidName)
        {
            return invalidName;
        }

        if (request.Address?.Trim().Length is not (> 0 and <= MaxAddressLength))
        {
            return Result.InvalidData($"Адрес обязателен и не длиннее {MaxAddressLength} символов");
        }

        return null;
    }

    private static void ApplyPickupPoint(PickupPoint point, PickupPointRequest request)
    {
        point.Name = request.Name.Trim();
        point.Address = request.Address.Trim();
    }

    private static Failure? CheckName(string? name)
    {
        if (name?.Trim().Length is not (> 0 and <= MaxNameLength))
        {
            return Result.InvalidData($"Название обязательно и не длиннее {MaxNameLength} символов");
        }

        return null;
    }

    private Task<bool> CategoryNameTaken(string name, int? ownId) =>
        db.Categories.AnyAsync(c => c.Name == name && c.Id != ownId);
}
