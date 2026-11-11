using Marketplace.Contracts;

namespace Marketplace.Api.Services.Admin;

public interface IAdminCatalogService
{
    Task<ProductDto> CreateProductAsync(ProductRequest request);

    Task<ProductDto> UpdateProductAsync(int id, ProductRequest request);

    Task<CategoryDto> CreateCategoryAsync(CategoryRequest request);

    Task<CategoryDto> UpdateCategoryAsync(int id, CategoryRequest request);

    Task<PickupPointDto> CreatePickupPointAsync(PickupPointRequest request);

    Task<PickupPointDto> UpdatePickupPointAsync(int id, PickupPointRequest request);

    Task DeletePickupPointAsync(int id);
}
