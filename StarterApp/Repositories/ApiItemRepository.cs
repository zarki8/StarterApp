using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using StarterApp.Database.Models;
using StarterApp.Models;
using StarterApp.Services;

namespace StarterApp.Repositories;

public class ApiItemRepository : IItemRepository
{
    private static readonly Dictionary<string, int> CategoryIds = new(StringComparer.OrdinalIgnoreCase)
    {
        ["tools"] = 1,
        ["camping"] = 2,
        ["sports"] = 3,
        ["electronics"] = 4,
        ["games"] = 5
    };

    private readonly HttpClient _httpClient;
    private readonly IApiTokenProvider _tokenProvider;

    public ApiItemRepository(HttpClient httpClient, IApiTokenProvider tokenProvider)
    {
        _httpClient = httpClient;
        _tokenProvider = tokenProvider;
    }

    public async Task<List<Item>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync("items?page=1&pageSize=100");
        await EnsureSuccessAsync(response);

        var result = await response.Content.ReadFromJsonAsync<ItemsResponse>();
        return result?.Items.Select(ToItem).ToList() ?? new List<Item>();
    }

    public async Task<Item?> GetByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"items/{id}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response);

        var item = await response.Content.ReadFromJsonAsync<ApiItemResponse>();
        return item == null ? null : ToItem(item);
    }

    public async Task<List<NearbyItem>> GetNearbyAsync(double latitude, double longitude, double radiusKm, string? category = null)
    {
        var query = string.Create(
            CultureInfo.InvariantCulture,
            $"items/nearby?lat={latitude:0.######}&lon={longitude:0.######}&radius={radiusKm:0.##}");

        if (!string.IsNullOrWhiteSpace(category))
        {
            query += $"&category={Uri.EscapeDataString(category.Trim().ToLowerInvariant())}";
        }

        var response = await _httpClient.GetAsync(query);
        await EnsureSuccessAsync(response);

        var result = await response.Content.ReadFromJsonAsync<NearbyItemsResponse>();
        return result?.Items.Select(ToNearbyItem).ToList() ?? new List<NearbyItem>();
    }

    public async Task<Item> AddAsync(Item item)
    {
        await ApplyBearerTokenAsync();

        var (latitude, longitude) = ParseLocation(item.Location);
        var request = new
        {
            title = item.Title,
            description = item.Description,
            dailyRate = item.DailyRate,
            categoryId = ParseCategoryId(item.Category),
            latitude,
            longitude
        };

        var response = await _httpClient.PostAsJsonAsync("items", request);
        await EnsureSuccessAsync(response);

        var created = await response.Content.ReadFromJsonAsync<ApiItemResponse>();
        return created == null ? item : ToItem(created);
    }

    public async Task UpdateAsync(Item item)
    {
        await ApplyBearerTokenAsync();

        var request = new
        {
            title = item.Title,
            description = item.Description,
            dailyRate = item.DailyRate,
            isAvailable = item.IsActive
        };

        var response = await _httpClient.PutAsJsonAsync($"items/{item.Id}", request);
        await EnsureSuccessAsync(response);
    }

    private async Task ApplyBearerTokenAsync()
    {
        var token = await _tokenProvider.GetValidTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("You must be logged in to save items.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static int ParseCategoryId(string category)
    {
        if (int.TryParse(category, NumberStyles.Integer, CultureInfo.InvariantCulture, out var categoryId))
        {
            return categoryId;
        }

        if (CategoryIds.TryGetValue(category.Trim(), out categoryId))
        {
            return categoryId;
        }

        throw new InvalidOperationException("Category must be one of: Tools, Camping, Sports, Electronics, Games.");
    }

    private static (decimal Latitude, decimal Longitude) ParseLocation(string location)
    {
        var parts = location.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2 &&
            decimal.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude) &&
            decimal.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude))
        {
            return (latitude, longitude);
        }

        throw new InvalidOperationException("Location must be coordinates in this format: 55.9533, -3.1883.");
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

    private static Item ToItem(ApiItemResponse response)
    {
        var (firstName, lastName) = SplitOwnerName(response.OwnerName);

        return new Item
        {
            Id = response.Id,
            Title = response.Title,
            Description = response.Description ?? string.Empty,
            DailyRate = response.DailyRate,
            Category = response.Category ?? response.CategoryId.ToString(CultureInfo.InvariantCulture),
            Location = FormatLocation(response.Latitude, response.Longitude),
            OwnerId = response.OwnerId,
            Owner = new User
            {
                Id = response.OwnerId,
                FirstName = firstName,
                LastName = lastName,
                IsActive = true
            },
            CreatedAt = response.CreatedAt,
            IsActive = response.IsAvailable
        };
    }

    private static NearbyItem ToNearbyItem(ApiNearbyItemResponse response)
    {
        return new NearbyItem
        {
            Id = response.Id,
            Title = response.Title,
            Description = response.Description ?? string.Empty,
            DailyRate = response.DailyRate,
            Category = response.Category ?? response.CategoryId.ToString(CultureInfo.InvariantCulture),
            OwnerId = response.OwnerId,
            OwnerName = response.OwnerName ?? "Unknown owner",
            Latitude = response.Latitude,
            Longitude = response.Longitude,
            DistanceKm = response.Distance,
            IsAvailable = response.IsAvailable,
            AverageRating = response.AverageRating,
            ImageUrl = response.ImageUrl ?? string.Empty
        };
    }

    private static string FormatLocation(decimal? latitude, decimal? longitude)
    {
        if (latitude == null || longitude == null)
        {
            return string.Empty;
        }

        return string.Create(
            CultureInfo.InvariantCulture,
            $"{latitude:0.######}, {longitude:0.######}");
    }

    private static (string FirstName, string LastName) SplitOwnerName(string? ownerName)
    {
        if (string.IsNullOrWhiteSpace(ownerName))
        {
            return ("Unknown", "owner");
        }

        var parts = ownerName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 1 ? (parts[0], string.Empty) : (parts[0], parts[1]);
    }

    private record ItemsResponse(List<ApiItemResponse> Items);

    private record NearbyItemsResponse(List<ApiNearbyItemResponse> Items);

    private record ApiItemResponse(
        int Id,
        string Title,
        string? Description,
        decimal DailyRate,
        int CategoryId,
        string? Category,
        int OwnerId,
        string? OwnerName,
        decimal? Latitude,
        decimal? Longitude,
        bool IsAvailable,
        DateTime? CreatedAt);

    private record ApiNearbyItemResponse(
        int Id,
        string Title,
        string? Description,
        decimal DailyRate,
        int CategoryId,
        string? Category,
        int OwnerId,
        string? OwnerName,
        decimal? Latitude,
        decimal? Longitude,
        decimal? Distance,
        bool IsAvailable,
        decimal? AverageRating,
        string? ImageUrl);

    private record ApiErrorResponse(string Error, string Message);
}
