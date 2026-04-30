using StarterApp.Models;

namespace StarterApp.Services;

public interface IReviewService
{
    Task<Review> SubmitAsync(int rentalId, int rating, string? comment);

    Task<List<Review>> GetForItemAsync(int itemId);

    Task<List<Review>> GetForUserAsync(int userId);
}
