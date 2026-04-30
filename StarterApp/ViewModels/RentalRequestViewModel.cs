using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Repositories;
using StarterApp.Services;

namespace StarterApp.ViewModels;

[QueryProperty(nameof(ItemId), "itemId")]
public partial class RentalRequestViewModel : BaseViewModel
{
    private readonly IItemRepository _itemRepository;
    private readonly IRentalRepository _rentalRepository;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private int itemId;

    [ObservableProperty]
    private string itemTitle = string.Empty;

    [ObservableProperty]
    private decimal dailyRate;

    [ObservableProperty]
    private DateTime startDate = DateTime.Today;

    [ObservableProperty]
    private DateTime endDate = DateTime.Today.AddDays(1);

    [ObservableProperty]
    private string ownerName = string.Empty;

    [ObservableProperty]
    private string successMessage = string.Empty;

    public DateTime MinimumDate => DateTime.Today;

    public string DailyRateDisplay => $"£{DailyRate:0.00}/day";

    public string EstimatedTotalDisplay
    {
        get
        {
            var days = Math.Max(1, (EndDate.Date - StartDate.Date).Days);
            return $"Estimated total: £{DailyRate * days:0.00}";
        }
    }

    public RentalRequestViewModel(
        IItemRepository itemRepository,
        IRentalRepository rentalRepository,
        INavigationService navigationService)
    {
        _itemRepository = itemRepository;
        _rentalRepository = rentalRepository;
        _navigationService = navigationService;
        Title = "Request Rental";
    }

    partial void OnItemIdChanged(int value)
    {
        _ = LoadItemAsync();
    }

    partial void OnDailyRateChanged(decimal value)
    {
        OnPropertyChanged(nameof(DailyRateDisplay));
        OnPropertyChanged(nameof(EstimatedTotalDisplay));
    }

    partial void OnStartDateChanged(DateTime value)
    {
        if (EndDate <= value)
        {
            EndDate = value.AddDays(1);
        }

        OnPropertyChanged(nameof(EstimatedTotalDisplay));
    }

    partial void OnEndDateChanged(DateTime value)
    {
        OnPropertyChanged(nameof(EstimatedTotalDisplay));
    }

    [RelayCommand]
    private async Task LoadItemAsync()
    {
        if (IsBusy || ItemId <= 0)
            return;

        try
        {
            IsBusy = true;
            ClearError();
            SuccessMessage = string.Empty;

            var item = await _itemRepository.GetByIdAsync(ItemId);
            if (item == null)
            {
                SetError("Item not found.");
                return;
            }

            ItemTitle = item.Title;
            DailyRate = item.DailyRate;
            OwnerName = item.Owner?.FullName ?? "Unknown owner";
        }
        catch (Exception ex)
        {
            SetError($"Failed to load item: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SubmitRequestAsync()
    {
        if (!ValidateDates())
            return;

        try
        {
            IsBusy = true;
            ClearError();
            SuccessMessage = string.Empty;

            await _rentalRepository.RequestRentalAsync(ItemId, StartDate.Date, EndDate.Date);
            SuccessMessage = "Rental request sent.";
        }
        catch (Exception ex)
        {
            SetError($"Failed to request rental: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task BackAsync()
    {
        await _navigationService.NavigateBackAsync();
    }

    [RelayCommand]
    private async Task ViewRentalsAsync()
    {
        await _navigationService.NavigateToAsync("RentalsPage");
    }

    private bool ValidateDates()
    {
        ClearError();

        if (StartDate.Date < DateTime.Today)
        {
            SetError("Start date must be today or later.");
            return false;
        }

        if (EndDate.Date <= StartDate.Date)
        {
            SetError("End date must be after the start date.");
            return false;
        }

        return true;
    }
}
