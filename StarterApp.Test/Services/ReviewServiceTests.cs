using StarterApp.Models;
using StarterApp.Repositories;
using StarterApp.Services;

namespace StarterApp.Test.Services;

public class ReviewServiceTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task SubmitAsync_InvalidRating_ThrowsValidationError(int rating)
    {
        // Arrange
        var service = new ReviewService(new FakeReviewRepository());

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SubmitAsync(12, rating, "Good item"));

        // Assert
        Assert.Equal("Rating must be between 1 and 5.", exception.Message);
    }

    [Fact]
    public async Task SubmitAsync_LongComment_ThrowsValidationError()
    {
        // Arrange
        var service = new ReviewService(new FakeReviewRepository());
        var longComment = new string('x', 501);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SubmitAsync(12, 5, longComment));

        // Assert
        Assert.Equal("Comment must be 500 characters or fewer.", exception.Message);
    }

    [Fact]
    public async Task SubmitAsync_ValidReview_TrimsCommentAndCallsRepository()
    {
        // Arrange
        var repository = new FakeReviewRepository();
        var service = new ReviewService(repository);

        // Act
        await service.SubmitAsync(12, 5, " Great owner ");

        // Assert
        Assert.Equal(12, repository.RentalId);
        Assert.Equal(5, repository.Rating);
        Assert.Equal("Great owner", repository.Comment);
    }

    private sealed class FakeReviewRepository : IReviewRepository
    {
        public int RentalId { get; private set; }
        public int Rating { get; private set; }
        public string? Comment { get; private set; }

        public Task<Review> SubmitAsync(int rentalId, int rating, string? comment)
        {
            RentalId = rentalId;
            Rating = rating;
            Comment = comment;
            return Task.FromResult(new Review { Id = 1, RentalId = rentalId, Rating = rating, Comment = comment ?? string.Empty });
        }

        public Task<List<Review>> GetForItemAsync(int itemId, int page = 1, int pageSize = 10) =>
            Task.FromResult(new List<Review>());

        public Task<List<Review>> GetForUserAsync(int userId, int page = 1, int pageSize = 10) =>
            Task.FromResult(new List<Review>());
    }
}
