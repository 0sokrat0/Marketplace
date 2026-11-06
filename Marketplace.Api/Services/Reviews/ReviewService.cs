using Marketplace.Api.Entities;
using Marketplace.Api.Infrastructure;
using Marketplace.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Api.Services.Reviews;

public class ReviewService(MarketplaceDbContext db) : IReviewService
{
    private const int MaxTextLength = 1000;

    public async Task<List<ReviewDto>> GetAsync(int productId)
    {
        await EnsureProductExists(productId);
        return await db.Reviews
            .AsNoTracking()
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewDto(r.Id, r.ProductId, r.User.Login, r.Rating, r.Text, r.CreatedAt))
            .ToListAsync();
    }

    public async Task<ReviewDto> SaveAsync(int userId, int productId, ReviewRequest request)
    {
        if (request.Rating is < OrderRules.MinRating or > OrderRules.MaxRating)
        {
            throw AppException.BadRequest($"Оценка от {OrderRules.MinRating} до {OrderRules.MaxRating}");
        }

        var text = request.Text?.Trim() ?? string.Empty;
        if (text.Length > MaxTextLength)
        {
            throw AppException.BadRequest($"Отзыв не длиннее {MaxTextLength} символов");
        }

        await EnsureProductExists(productId);

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

    public async Task DeleteAsync(int reviewId)
    {
        var review = await db.Reviews.FindAsync(reviewId)
            ?? throw AppException.NotFound("Отзыв не найден");
        db.Reviews.Remove(review);
        await db.SaveChangesAsync();
    }

    private async Task EnsureProductExists(int productId)
    {
        if (!await db.Products.AnyAsync(p => p.Id == productId))
        {
            throw AppException.NotFound("Товар не найден");
        }
    }
}
