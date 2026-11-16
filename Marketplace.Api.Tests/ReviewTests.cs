using System.Net;
using Marketplace.Contracts;

namespace Marketplace.Api.Tests;

public class ReviewTests : ApiTestBase
{
    [Fact]
    public async Task AverageRating_IsCalculatedFromAllUsers()
    {
        var first = await User("first");
        var second = await User("second");

        await first.Post($"api/Review/Product/{Mug}", new ReviewRequest(5, "Отлично"));
        await second.Post($"api/Review/Product/{Mug}", new ReviewRequest(2, "Так себе"));

        var product = (await Guest().Get($"api/Product/{Mug}")).Json<ProductDto>();
        Assert.Equal(3.5, product.AverageRating);
        Assert.Equal(2, product.ReviewCount);
    }

    [Fact]
    public async Task SecondReviewFromSameUser_ReplacesTheFirst()
    {
        var user = await User();

        await user.Post($"api/Review/Product/{Mug}", new ReviewRequest(5, "Отлично"));
        await user.Post($"api/Review/Product/{Mug}", new ReviewRequest(1, "Передумал"));

        var reviews = (await Guest().Get($"api/Review/Product/{Mug}")).Json<List<ReviewDto>>();
        Assert.Single(reviews);
        Assert.Equal(1, reviews[0].Rating);
    }

    [Fact]
    public async Task Guest_CannotWriteReview()
    {
        var result = await Guest().Post($"api/Review/Product/{Mug}", new ReviewRequest(5, "Привет"));

        Assert.Equal(HttpStatusCode.Unauthorized, result.Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task RatingOutsideRange_IsRejected(int rating)
    {
        var user = await User();

        var result = await user.Post($"api/Review/Product/{Mug}", new ReviewRequest(rating, "Текст"));

        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
    }
}
