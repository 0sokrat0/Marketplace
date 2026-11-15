using Marketplace.Api.Infrastructure;
using Marketplace.Api.Services.Reviews;
using Marketplace.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReviewController(IReviewService reviews) : ControllerBase
{
    [HttpGet("Product/{productId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByProduct([FromRoute] int productId) =>
        (await reviews.GetAsync(productId)).ToActionResult();

    [HttpPost("Product/{productId:int}")]
    public async Task<IActionResult> Save([FromRoute] int productId, [FromBody] ReviewRequest request) =>
        (await reviews.SaveAsync(User.GetUserId(), productId, request)).ToActionResult();

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete([FromRoute] int id) =>
        (await reviews.DeleteAsync(id)).ToActionResult();
}
