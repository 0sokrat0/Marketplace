using Marketplace.Api.Services.Admin;
using Marketplace.Api.Services.Catalog;
using Marketplace.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoryController(ICatalogService catalog, IAdminCatalogService admin) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll() => (await catalog.GetCategoriesAsync()).ToActionResult();

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create([FromBody] CategoryRequest request) =>
        (await admin.CreateCategoryAsync(request)).ToActionResult();

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Put([FromRoute] int id, [FromBody] CategoryRequest request) =>
        (await admin.UpdateCategoryAsync(id, request)).ToActionResult();
}
