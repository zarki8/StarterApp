using System.Net.Http.Headers;
using System.Net.Http.Json;
using StarterApp.Models;
using StarterApp.Services;

namespace StarterApp.Repositories;

public class ApiReviewRepository : IReviewRepository
{
    private readonly HttpClient _httpClient;
    private readonly IApiTokenProvider _tokenProvider;

    public ApiReviewRepository(HttpClient httpClient, IApiTokenProvider tokenProvider)
    {
        _httpClient = httpClient;
        _tokenProvider = tokenProvider;
    }

    public async Task<Review> SubmitAsync(int rentalId, int rating, string? comment)
    {
        await ApplyBearerTokenAsync();

        var response = await _httpClient.PostAsJsonAsync("reviews", new
        {
            rentalId,
            rating,
            comment = comment ?? string.Empty
        });

        await EnsureSuccessAsync(response);

        var review = await response.Content.ReadFromJsonAsync<ApiReviewResponse>();
        return review == null ? new Review() : ToReview(review);
    }

    public async Task<List<Review>> GetForItemAsync(int itemId, int page = 1, int pageSize = 10)
    {
        var response = await _httpClient.GetAsync($"items/{itemId}/reviews?page={page}&pageSize={pageSize}");
        await EnsureSuccessAsync(response);

        var result = await response.Content.ReadFromJsonAsync<ReviewsResponse>();
        return result?.Reviews.Select(ToReview).ToList() ?? new List<Review>();
    }

    public async Task<List<Review>> GetForUserAsync(int userId, int page = 1, int pageSize = 10)
    {
        var response = await _httpClient.GetAsync($"users/{userId}/reviews?page={page}&pageSize={pageSize}");
        await EnsureSuccessAsync(response);

        var result = await response.Content.ReadFromJsonAsync<ReviewsResponse>();
        return result?.Reviews.Select(ToReview).ToList() ?? new List<Review>();
    }

    private async Task ApplyBearerTokenAsync()
    {
        var token = await _tokenProvider.GetValidTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("You must be logged in to submit reviews.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        throw new InvalidOperationException(error?.Message ?? $"API request failed with status {(int)response.StatusCode}.");
    }

    private static Review ToReview(ApiReviewResponse response)
    {
        return new Review
        {
            Id = response.Id,
            RentalId = response.RentalId,
            ItemId = response.ItemId,
            ItemTitle = response.ItemTitle ?? string.Empty,
            ReviewerId = response.ReviewerId,
            ReviewerName = response.ReviewerName,
            Rating = response.Rating,
            Comment = response.Comment ?? string.Empty,
            CreatedAt = response.CreatedAt
        };
    }

    private record ReviewsResponse(List<ApiReviewResponse> Reviews);

    private record ApiReviewResponse(
        int Id,
        int? RentalId,
        int? ItemId,
        string? ItemTitle,
        int ReviewerId,
        string ReviewerName,
        int Rating,
        string? Comment,
        DateTime CreatedAt);

    private record ApiErrorResponse(string Error, string Message);
}
