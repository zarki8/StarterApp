using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Models;
using StarterApp.Repositories;
using StarterApp.Services;

namespace StarterApp.ViewModels;

public partial class NearbyItemsViewModel : BaseViewModel
{
    private readonly IItemRepository _itemRepository;
    private readonly ILocationService _locationService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<NearbyItemListItem> items = new();

    [ObservableProperty]
    private string latitudeText = "55.9533";

    [ObservableProperty]
    private string longitudeText = "-3.1883";

    [ObservableProperty]
    private double radiusKm = 5;

    [ObservableProperty]
    private string category = string.Empty;

    [ObservableProperty]
    private string locationSummary = "Edinburgh city centre";

    public NearbyItemsViewModel(
        IItemRepository itemRepository,
        ILocationService locationService,
        INavigationService navigationService)
    {
        _itemRepository = itemRepository;
        _locationService = locationService;
        _navigationService = navigationService;
        Title = "Find Near Me";
    }

    public string RadiusDisplay => $"{RadiusKm:0} km";

    partial void OnRadiusKmChanged(double value)
    {
        OnPropertyChanged(nameof(RadiusDisplay));
    }

    [RelayCommand]
    private async Task UseCurrentLocationAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            var location = await _locationService.GetCurrentLocationAsync();
            LatitudeText = location.Latitude.ToString("0.######", CultureInfo.InvariantCulture);
            LongitudeText = location.Longitude.ToString("0.######", CultureInfo.InvariantCulture);
            LocationSummary = "Current device location";
        }
        catch (Exception ex)
        {
            SetError($"Could not get location: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchNearbyAsync()
    {
        if (IsBusy)
            return;

        if (!TryReadCoordinates(out var latitude, out var longitude))
            return;

        try
        {
            IsBusy = true;
            ClearError();

            var nearbyItems = await _itemRepository.GetNearbyAsync(
                latitude,
                longitude,
                RadiusKm,
                string.IsNullOrWhiteSpace(Category) ? null : Category);

            Items = new ObservableCollection<NearbyItemListItem>(
                nearbyItems.Select(NearbyItemListItem.FromNearbyItem));

            LocationSummary = $"{latitude:0.####}, {longitude:0.####} within {RadiusKm:0} km";
        }
        catch (Exception ex)
        {
            SetError($"Failed to find nearby items: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ViewItemAsync(NearbyItemListItem item)
    {
        if (item == null)
            return;

        await _navigationService.NavigateToAsync($"ItemDetailPage?itemId={item.Id}");
    }

    [RelayCommand]
    private async Task NavigateToDashboardAsync()
    {
        await _navigationService.NavigateToAsync("MainPage");
    }

    private bool TryReadCoordinates(out double latitude, out double longitude)
    {
        ClearError();

        if (!double.TryParse(LatitudeText, NumberStyles.Float, CultureInfo.InvariantCulture, out latitude) ||
            latitude < -90 ||
            latitude > 90)
        {
            SetError("Latitude must be between -90 and 90.");
            longitude = 0;
            return false;
        }

        if (!double.TryParse(LongitudeText, NumberStyles.Float, CultureInfo.InvariantCulture, out longitude) ||
            longitude < -180 ||
            longitude > 180)
        {
            SetError("Longitude must be between -180 and 180.");
            return false;
        }

        return true;
    }
}

public class NearbyItemListItem
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string OwnerName { get; set; } = string.Empty;

    public decimal DailyRate { get; set; }

    public decimal? DistanceKm { get; set; }

    public string DailyRateDisplay => $"£{DailyRate:0.00}/day";

    public string DistanceDisplay => DistanceKm == null ? "Distance unavailable" : $"{DistanceKm:0.0} km away";

    public static NearbyItemListItem FromNearbyItem(NearbyItem item)
    {
        return new NearbyItemListItem
        {
            Id = item.Id,
            Title = item.Title,
            Description = item.Description,
            Category = item.Category,
            OwnerName = item.OwnerName,
            DailyRate = item.DailyRate,
            DistanceKm = item.DistanceKm
        };
    }
}
