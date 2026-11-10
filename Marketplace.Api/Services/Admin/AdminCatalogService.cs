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

    public async Task<ProductDto> CreateProductAsync(ProductRequest request)
    {
        await ValidateProduct(request);
        var product = new Product();
        Apply(product, request);
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return await catalog.GetProductAsync(product.Id);
    }

    public async Task<ProductDto> UpdateProductAsync(int id, ProductRequest request)
    {
        var product = await db.Products.FindAsync(id) ?? throw AppException.NotFound("Товар не найден");
        await ValidateProduct(request);
        Apply(product, request);
        await db.SaveChangesAsync();
        return await catalog.GetProductAsync(product.Id);
    }

    public async Task<CategoryDto> CreateCategoryAsync(CategoryRequest request)
    {
        var name = ValidateName(request.Name);
        await EnsureCategoryNameFree(name, null);
        var category = new Category { Name = name };
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return new CategoryDto(category.Id, category.Name);
    }

    public async Task<CategoryDto> UpdateCategoryAsync(int id, CategoryRequest request)
    {
        var category = await db.Categories.FindAsync(id) ?? throw AppException.NotFound("Категория не найдена");
        var name = ValidateName(request.Name);
        await EnsureCategoryNameFree(name, id);
        category.Name = name;
        await db.SaveChangesAsync();
        return new CategoryDto(category.Id, category.Name);
    }

    public async Task<PickupPointDto> CreatePickupPointAsync(PickupPointRequest request)
    {
        var point = new PickupPoint();
        ApplyPickupPoint(point, request);
        db.PickupPoints.Add(point);
        await db.SaveChangesAsync();
        return new PickupPointDto(point.Id, point.Name, point.Address);
    }

    public async Task<PickupPointDto> UpdatePickupPointAsync(int id, PickupPointRequest request)
    {
        var point = await db.PickupPoints.FindAsync(id) ?? throw AppException.NotFound("Пункт выдачи не найден");
        ApplyPickupPoint(point, request);
        await db.SaveChangesAsync();
        return new PickupPointDto(point.Id, point.Name, point.Address);
    }

    public async Task DeletePickupPointAsync(int id)
    {
        var point = await db.PickupPoints.FindAsync(id) ?? throw AppException.NotFound("Пункт выдачи не найден");
        if (await db.Orders.AnyAsync(o => o.PickupPointId == id))
        {
            throw AppException.Conflict("Пункт выдачи используется в заказах, удалить его нельзя");
        }

        db.PickupPoints.Remove(point);
        await db.SaveChangesAsync();
    }

    private async Task ValidateProduct(ProductRequest request)
    {
        ValidateName(request.Name);
        if (request.Description?.Length > MaxDescriptionLength)
        {
            throw AppException.BadRequest($"Описание не длиннее {MaxDescriptionLength} символов");
        }

        if (request.Price < 0)
        {
            throw AppException.BadRequest("Цена не может быть отрицательной");
        }

        if (request.Stock < 0)
        {
            throw AppException.BadRequest("Остаток на складе не может быть отрицательным");
        }

        if (request.PackSize is <= 0)
        {
            throw AppException.BadRequest("Количество штук в упаковке должно быть больше нуля");
        }

        if (!await db.Categories.AnyAsync(c => c.Id == request.CategoryId))
        {
            throw AppException.BadRequest("Выберите существующую категорию");
        }
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

    private static void ApplyPickupPoint(PickupPoint point, PickupPointRequest request)
    {
        point.Name = ValidateName(request.Name);
        var address = request.Address?.Trim() ?? string.Empty;
        if (address.Length is 0 or > MaxAddressLength)
        {
            throw AppException.BadRequest($"Адрес обязателен и не длиннее {MaxAddressLength} символов");
        }

        point.Address = address;
    }

    private static string ValidateName(string? name)
    {
        var trimmed = name?.Trim() ?? string.Empty;
        if (trimmed.Length is 0 or > MaxNameLength)
        {
            throw AppException.BadRequest($"Название обязательно и не длиннее {MaxNameLength} символов");
        }

        return trimmed;
    }

    private async Task EnsureCategoryNameFree(string name, int? ownId)
    {
        if (await db.Categories.AnyAsync(c => c.Name == name && c.Id != ownId))
        {
            throw AppException.Conflict("Категория с таким названием уже есть");
        }
    }
}
