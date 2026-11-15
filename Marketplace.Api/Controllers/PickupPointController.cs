using Marketplace.Api.Services.Admin;
using Marketplace.Api.Services.Catalog;
using Marketplace.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PickupPointController(ICatalogService catalog, IAdminCatalogService admin) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll() => (await catalog.GetPickupPointsAsync()).ToActionResult();

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create([FromBody] PickupPointRequest request) =>
        (await admin.CreatePickupPointAsync(request)).ToActionResult();

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Put([FromRoute] int id, [FromBody] PickupPointRequest request) =>
        (await admin.UpdatePickupPointAsync(id, request)).ToActionResult();

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete([FromRoute] int id) =>
        (await admin.DeletePickupPointAsync(id)).ToActionResult();
}
