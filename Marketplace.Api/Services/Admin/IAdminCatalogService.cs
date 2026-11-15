using Marketplace.Api.Infrastructure;
using Marketplace.Contracts;

namespace Marketplace.Api.Services.Admin;

public interface IAdminCatalogService
{
    Task<Result<ProductDto>> CreateProductAsync(ProductRequest request);

    Task<Result<ProductDto>> UpdateProductAsync(int id, ProductRequest request);

    Task<Result<CategoryDto>> CreateCategoryAsync(CategoryRequest request);

    Task<Result<CategoryDto>> UpdateCategoryAsync(int id, CategoryRequest request);

    Task<Result<PickupPointDto>> CreatePickupPointAsync(PickupPointRequest request);

    Task<Result<PickupPointDto>> UpdatePickupPointAsync(int id, PickupPointRequest request);

    Task<Result> DeletePickupPointAsync(int id);
}
