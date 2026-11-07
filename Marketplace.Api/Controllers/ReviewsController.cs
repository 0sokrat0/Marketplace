using Marketplace.Api.Infrastructure;
using Marketplace.Api.Services.Reviews;
using Marketplace.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/products/{productId:int}/reviews")]
public class ReviewsController(IReviewService reviews) : ControllerBase
{
    [HttpGet]
    public Task<List<ReviewDto>> Get(int productId) => reviews.GetAsync(productId);

    [HttpPost]
    [Authorize]
    public Task<ReviewDto> Save(int productId, ReviewRequest request) =>
        reviews.SaveAsync(User.GetUserId(), productId, request);
}
