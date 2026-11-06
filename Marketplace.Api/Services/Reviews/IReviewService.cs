using Marketplace.Contracts;

namespace Marketplace.Api.Services.Reviews;

public interface IReviewService
{
    Task<List<ReviewDto>> GetAsync(int productId);

    Task<ReviewDto> SaveAsync(int userId, int productId, ReviewRequest request);

    Task DeleteAsync(int reviewId);
}
