using StarterApp.Models;
using StarterApp.Repositories;

namespace StarterApp.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;

    public ReviewService(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public Task<Review> SubmitAsync(int rentalId, int rating, string? comment)
    {
        ValidateReview(rentalId, rating, comment);
        return _reviewRepository.SubmitAsync(rentalId, rating, comment?.Trim());
    }

    public Task<List<Review>> GetForItemAsync(int itemId)
    {
        if (itemId <= 0)
        {
            throw new InvalidOperationException("An item is required before loading reviews.");
        }

        return _reviewRepository.GetForItemAsync(itemId);
    }

    public Task<List<Review>> GetForUserAsync(int userId)
    {
        if (userId <= 0)
        {
            throw new InvalidOperationException("A user is required before loading reviews.");
        }

        return _reviewRepository.GetForUserAsync(userId);
    }

    private static void ValidateReview(int rentalId, int rating, string? comment)
    {
        if (rentalId <= 0)
        {
            throw new InvalidOperationException("A completed rental is required before submitting a review.");
        }

        if (rating < 1 || rating > 5)
        {
            throw new InvalidOperationException("Rating must be between 1 and 5.");
        }

        if (comment?.Length > 500)
        {
            throw new InvalidOperationException("Comment must be 500 characters or fewer.");
        }
    }
}
