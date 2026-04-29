namespace StarterApp.Services;

public interface IApiTokenProvider
{
    Task<string?> GetValidTokenAsync();
}