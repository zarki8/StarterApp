using StarterApp.Models;

namespace StarterApp.Repositories;

public interface IReviewRepository
{
    Task<Review> SubmitAsync(int rentalId, int rating, string? comment);

    Task<List<Review>> GetForItemAsync(int itemId, int page = 1, int pageSize = 10);

    Task<List<Review>> GetForUserAsync(int userId, int page = 1, int pageSize = 10);
}
