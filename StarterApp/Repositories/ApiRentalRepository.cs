using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using StarterApp.Models;
using StarterApp.Services;

namespace StarterApp.Repositories;

public class ApiRentalRepository : IRentalRepository
{
    private readonly HttpClient _httpClient;
    private readonly IApiTokenProvider _tokenProvider;

    public ApiRentalRepository(HttpClient httpClient, IApiTokenProvider tokenProvider)
    {
        _httpClient = httpClient;
        _tokenProvider = tokenProvider;
    }

    public async Task<Rental> RequestRentalAsync(int itemId, DateTime startDate, DateTime endDate)
    {
        await ApplyBearerTokenAsync();

        var request = new
        {
            itemId,
            startDate = startDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            endDate = endDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        };

        var response = await _httpClient.PostAsJsonAsync("rentals", request);
        await EnsureSuccessAsync(response);

        var rental = await response.Content.ReadFromJsonAsync<ApiRentalResponse>();
        return rental == null ? new Rental() : ToRental(rental);
    }

    public async Task<List<Rental>> GetIncomingAsync(string? status = null)
    {
        await ApplyBearerTokenAsync();

        var response = await _httpClient.GetAsync(BuildRentalsUrl("rentals/incoming", status));
        await EnsureSuccessAsync(response);

        var result = await response.Content.ReadFromJsonAsync<RentalsResponse>();
        return result?.Rentals.Select(ToRental).ToList() ?? new List<Rental>();
    }

    public async Task<List<Rental>> GetOutgoingAsync(string? status = null)
    {
        await ApplyBearerTokenAsync();

        var response = await _httpClient.GetAsync(BuildRentalsUrl("rentals/outgoing", status));
        await EnsureSuccessAsync(response);

        var result = await response.Content.ReadFromJsonAsync<RentalsResponse>();
        return result?.Rentals.Select(ToRental).ToList() ?? new List<Rental>();
    }

    private async Task ApplyBearerTokenAsync()
    {
        var token = await _tokenProvider.GetValidTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("You must be logged in to manage rentals.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static string BuildRentalsUrl(string path, string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return path;
        }

        return $"{path}?status={Uri.EscapeDataString(status)}";
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

    private static Rental ToRental(ApiRentalResponse response)
    {
        return new Rental
        {
            Id = response.Id,
            ItemId = response.ItemId,
            ItemTitle = response.ItemTitle,
            BorrowerId = response.BorrowerId,
            BorrowerName = response.BorrowerName ?? string.Empty,
            OwnerId = response.OwnerId,
            OwnerName = response.OwnerName ?? string.Empty,
            StartDate = response.StartDate,
            EndDate = response.EndDate,
            Status = response.Status,
            TotalPrice = response.TotalPrice,
            RequestedAt = response.RequestedAt ?? response.CreatedAt,
            ApprovedAt = response.ApprovedAt
        };
    }

    private record RentalsResponse(List<ApiRentalResponse> Rentals);

    private record ApiRentalResponse(
        int Id,
        int ItemId,
        string ItemTitle,
        int? BorrowerId,
        string? BorrowerName,
        int? OwnerId,
        string? OwnerName,
        DateTime StartDate,
        DateTime EndDate,
        string Status,
        decimal TotalPrice,
        DateTime? RequestedAt,
        DateTime? ApprovedAt,
        DateTime? CreatedAt);

    private record ApiErrorResponse(string Error, string Message);
}
