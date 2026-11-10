using Marketplace.Api.Services.Admin;
using Marketplace.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Api.Controllers;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin")]
public class AdminCatalogController(IAdminCatalogService admin) : ControllerBase
{
    [HttpPost("products")]
    public Task<ProductDto> CreateProduct(ProductRequest request) => admin.CreateProductAsync(request);

    [HttpPut("products/{id:int}")]
    public Task<ProductDto> UpdateProduct(int id, ProductRequest request) => admin.UpdateProductAsync(id, request);

    [HttpPost("categories")]
    public Task<CategoryDto> CreateCategory(CategoryRequest request) => admin.CreateCategoryAsync(request);

    [HttpPut("categories/{id:int}")]
    public Task<CategoryDto> UpdateCategory(int id, CategoryRequest request) => admin.UpdateCategoryAsync(id, request);

    [HttpPost("pickup-points")]
    public Task<PickupPointDto> CreatePickupPoint(PickupPointRequest request) => admin.CreatePickupPointAsync(request);

    [HttpPut("pickup-points/{id:int}")]
    public Task<PickupPointDto> UpdatePickupPoint(int id, PickupPointRequest request) =>
        admin.UpdatePickupPointAsync(id, request);

    [HttpDelete("pickup-points/{id:int}")]
    public async Task<IActionResult> DeletePickupPoint(int id)
    {
        await admin.DeletePickupPointAsync(id);
        return NoContent();
    }
}
