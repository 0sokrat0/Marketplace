using Marketplace.Api.Entities;
using Marketplace.Api.Infrastructure;
using Marketplace.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Api.Services.Reviews;

public class ReviewService(MarketplaceDbContext db) : IReviewService
{
    private const int MaxTextLength = 1000;

    public async Task<Result<List<ReviewDto>>> GetAsync(int productId)
    {
        if (!await db.Products.AnyAsync(p => p.Id == productId))
        {
            return Result.NotFound("Товар не найден");
        }

        return await db.Reviews
            .AsNoTracking()
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewDto(r.Id, r.ProductId, r.User.Login, r.Rating, r.Text, r.CreatedAt))
            .ToListAsync();
    }

    public async Task<Result<ReviewDto>> SaveAsync(int userId, int productId, ReviewRequest request)
    {
        if (request.Rating is < OrderRules.MinRating or > OrderRules.MaxRating)
        {
            return Result.InvalidData($"Оценка от {OrderRules.MinRating} до {OrderRules.MaxRating}");
        }

        var text = request.Text?.Trim() ?? string.Empty;
        if (text.Length > MaxTextLength)
        {
            return Result.InvalidData($"Отзыв не длиннее {MaxTextLength} символов");
        }

        if (!await db.Products.AnyAsync(p => p.Id == productId))
        {
            return Result.NotFound("Товар не найден");
        }

        var review = await db.Reviews.FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == userId);
        if (review is null)
        {
            review = new Review { ProductId = productId, UserId = userId };
            db.Reviews.Add(review);
        }

        review.Rating = request.Rating;
        review.Text = text;
        review.CreatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        var login = await db.Users.Where(u => u.Id == userId).Select(u => u.Login).SingleAsync();
        return new ReviewDto(review.Id, productId, login, review.Rating, review.Text, review.CreatedAt);
    }

    public async Task<Result> DeleteAsync(int reviewId)
    {
        var review = await db.Reviews.FindAsync(reviewId);
        if (review is null)
        {
            return Result.NotFound("Отзыв не найден");
        }

        db.Reviews.Remove(review);
        await db.SaveChangesAsync();
        return Result.NoContent();
    }
}
