using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Maui.Storage;
using StarterApp.Database.Models;

namespace StarterApp.Services;

public class ApiAuthenticationService : IAuthenticationService, IApiTokenProvider
{
    private const string TokenKey = "api_jwt_token";
    private const string TokenExpiryKey = "api_jwt_expires_at";

    private readonly HttpClient _httpClient;
    private User? _currentUser;
    private readonly List<string> _currentUserRoles = new();

    public event EventHandler<bool>? AuthenticationStateChanged;

    public bool IsAuthenticated => _currentUser != null;
    public User? CurrentUser => _currentUser;
    public List<string> CurrentUserRoles => _currentUserRoles;

    public ApiAuthenticationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AuthenticationResult> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("auth/token", new { email, password });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                return new AuthenticationResult(false, error?.Message ?? "Login failed");
            }

            var token = await response.Content.ReadFromJsonAsync<TokenResponse>();
            if (token == null)
            {
                return new AuthenticationResult(false, "Login failed: token response was empty");
            }

            await StoreTokenAsync(token.Token, token.ExpiresAt);
            ApplyBearerToken(token.Token);

            var meResponse = await _httpClient.GetAsync("users/me");
            if (!meResponse.IsSuccessStatusCode)
            {
                await ClearTokenAsync();
                return new AuthenticationResult(false, "Login failed: could not load profile");
            }

            var profile = await meResponse.Content.ReadFromJsonAsync<UserProfileResponse>();
            if (profile == null)
            {
                await ClearTokenAsync();
                return new AuthenticationResult(false, "Login failed: profile response was empty");
            }

            _currentUser = new User
            {
                Id = profile.Id,
                Email = profile.Email,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                CreatedAt = profile.CreatedAt,
                IsActive = true,
                AverageRating = profile.AverageRating
            };

            AuthenticationStateChanged?.Invoke(this, true);
            return new AuthenticationResult(true, "Login successful");
        }
        catch (Exception ex)
        {
            return new AuthenticationResult(false, $"Login failed: {ex.Message}");
        }
    }

    public async Task<AuthenticationResult> RegisterAsync(string firstName, string lastName, string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("auth/register", new
            {
                firstName,
                lastName,
                email,
                password
            });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                return new AuthenticationResult(false, error?.Message ?? "Registration failed");
            }

            return new AuthenticationResult(true, "Registration successful. Please log in.");
        }
        catch (Exception ex)
        {
            return new AuthenticationResult(false, $"Registration failed: {ex.Message}");
        }
    }

    public async Task<string?> GetValidTokenAsync()
    {
        var token = await SecureStorage.GetAsync(TokenKey);
        var expiresAtText = Preferences.Get(TokenExpiryKey, string.Empty);

        if (string.IsNullOrWhiteSpace(token) ||
            string.IsNullOrWhiteSpace(expiresAtText) ||
            !DateTime.TryParse(expiresAtText, out var expiresAt))
        {
            await ClearTokenAsync();
            return null;
        }

        if (DateTime.UtcNow >= expiresAt.ToUniversalTime())
        {
            await ClearTokenAsync();
            return null;
        }

        ApplyBearerToken(token);
        return token;
    }

    public async Task LogoutAsync()
    {
        _currentUser = null;
        _currentUserRoles.Clear();
        await ClearTokenAsync();
        AuthenticationStateChanged?.Invoke(this, false);
    }

    public bool HasRole(string roleName) =>
        _currentUserRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);

    public bool HasAnyRole(params string[] roleNames) =>
        roleNames.Any(HasRole);

    public bool HasAllRoles(params string[] roleNames) =>
        roleNames.All(HasRole);

    public Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        return Task.FromResult(false);
    }

    private async Task StoreTokenAsync(string token, DateTime expiresAt)
    {
        await SecureStorage.SetAsync(TokenKey, token);
        Preferences.Set(TokenExpiryKey, expiresAt.ToUniversalTime().ToString("O"));
    }

    private async Task ClearTokenAsync()
    {
        SecureStorage.Remove(TokenKey);
        Preferences.Remove(TokenExpiryKey);
        _httpClient.DefaultRequestHeaders.Authorization = null;
        await Task.CompletedTask;
    }

    private void ApplyBearerToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    private record TokenResponse(string Token, DateTime ExpiresAt, int UserId);

    private record UserProfileResponse(
        int Id,
        string Email,
        string FirstName,
        string LastName,
        double? AverageRating,
        int? ItemsListed,
        int? RentalsCompleted,
        DateTime CreatedAt);

    private record ApiErrorResponse(string Error, string Message);
}
