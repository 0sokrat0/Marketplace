using Marketplace.Contracts;

namespace Marketplace.Api.Services.Catalog;

public interface ICatalogService
{
    Task<List<CategoryDto>> GetCategoriesAsync();

    Task<List<ProductDto>> GetProductsAsync(ProductFilter filter);

    Task<ProductDto> GetProductAsync(int id);

    Task<List<ProductDto>> GetByIdsAsync(IReadOnlyCollection<int> ids);
}
