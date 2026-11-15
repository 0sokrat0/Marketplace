using Marketplace.Api.Services.Admin;
using Marketplace.Api.Services.Catalog;
using Marketplace.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductController(ICatalogService catalog, IAdminCatalogService admin) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] ProductFilter filter) =>
        (await catalog.GetProductsAsync(filter)).ToActionResult();

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById([FromRoute] int id) =>
        (await catalog.GetProductAsync(id)).ToActionResult();

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create([FromBody] ProductRequest request) =>
        (await admin.CreateProductAsync(request)).ToActionResult();

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Put([FromRoute] int id, [FromBody] ProductRequest request) =>
        (await admin.UpdateProductAsync(id, request)).ToActionResult();
}
