using Marketplace.Api.Services.Catalog;
using Marketplace.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Api.Controllers;

[ApiController]
public class CatalogController(ICatalogService catalog) : ControllerBase
{
    [HttpGet("api/categories")]
    public Task<List<CategoryDto>> Categories() => catalog.GetCategoriesAsync();

    [HttpGet("api/products")]
    public Task<List<ProductDto>> Products([FromQuery] ProductFilter filter) => catalog.GetProductsAsync(filter);

    [HttpGet("api/products/{id:int}")]
    public Task<ProductDto> Product(int id) => catalog.GetProductAsync(id);
}
