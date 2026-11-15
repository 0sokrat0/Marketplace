using Marketplace.Api.Infrastructure;
using Marketplace.Contracts;

namespace Marketplace.Api.Services.Reviews;

public interface IReviewService
{
    Task<Result<List<ReviewDto>>> GetAsync(int productId);

    Task<Result<ReviewDto>> SaveAsync(int userId, int productId, ReviewRequest request);

    Task<Result> DeleteAsync(int reviewId);
}
