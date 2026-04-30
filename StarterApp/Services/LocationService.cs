using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;

namespace StarterApp.Services;

public class LocationService : ILocationService
{
    public async Task<UserLocation> GetCurrentLocationAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        if (status != PermissionStatus.Granted)
        {
            throw new InvalidOperationException("Location permission is required to find nearby items.");
        }

        var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
        var location = await Geolocation.Default.GetLocationAsync(request);

        if (location == null)
        {
            throw new InvalidOperationException("Could not get your current location.");
        }

        return new UserLocation(location.Latitude, location.Longitude);
    }
}
