namespace StarterApp.Services;

public interface ILocationService
{
    Task<UserLocation> GetCurrentLocationAsync();
}
