using Marketplace.Api.Infrastructure;
using Marketplace.Contracts;

namespace Marketplace.Api.Services.Catalog;

public interface ICatalogService
{
    Task<Result<List<CategoryDto>>> GetCategoriesAsync();

    Task<Result<List<PickupPointDto>>> GetPickupPointsAsync();

    Task<Result<List<ProductDto>>> GetProductsAsync(ProductFilter filter);

    Task<Result<ProductDto>> GetProductAsync(int id);

    Task<List<ProductDto>> GetByIdsAsync(IReadOnlyCollection<int> ids);
}
