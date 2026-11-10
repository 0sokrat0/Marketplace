using Marketplace.Api.Services.Reviews;
using Marketplace.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Api.Controllers;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin/reviews")]
public class AdminReviewsController(IReviewService reviews) : ControllerBase
{
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await reviews.DeleteAsync(id);
        return NoContent();
    }
}
