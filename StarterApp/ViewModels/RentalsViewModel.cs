using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Models;
using StarterApp.Repositories;
using StarterApp.Services;

namespace StarterApp.ViewModels;

public partial class RentalsViewModel : BaseViewModel
{
    private readonly IRentalRepository _rentalRepository;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<RentalListItem> incomingRentals = new();

    [ObservableProperty]
    private ObservableCollection<RentalListItem> outgoingRentals = new();

    public RentalsViewModel(
        IRentalRepository rentalRepository,
        INavigationService navigationService)
    {
        _rentalRepository = rentalRepository;
        _navigationService = navigationService;
        Title = "Rentals";
    }

    [RelayCommand]
    private async Task LoadRentalsAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            var incoming = await _rentalRepository.GetIncomingAsync();
            var outgoing = await _rentalRepository.GetOutgoingAsync();

            IncomingRentals = new ObservableCollection<RentalListItem>(
                incoming.Select(rental => RentalListItem.FromRental(rental, "Borrower")));

            OutgoingRentals = new ObservableCollection<RentalListItem>(
                outgoing.Select(rental => RentalListItem.FromRental(rental, "Owner")));
        }
        catch (Exception ex)
        {
            SetError($"Failed to load rentals: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToDashboardAsync()
    {
        await _navigationService.NavigateToAsync("MainPage");
    }
}

public class RentalListItem
{
    public int Id { get; set; }

    public string ItemTitle { get; set; } = string.Empty;

    public string PersonLabel { get; set; } = string.Empty;

    public string PersonName { get; set; } = string.Empty;

    public string PersonDisplay => $"{PersonLabel}: {PersonName}";

    public string DateRange { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string TotalPrice { get; set; } = string.Empty;

    public static RentalListItem FromRental(Rental rental, string personLabel)
    {
        var personName = personLabel == "Owner" ? rental.OwnerName : rental.BorrowerName;

        return new RentalListItem
        {
            Id = rental.Id,
            ItemTitle = rental.ItemTitle,
            PersonLabel = personLabel,
            PersonName = string.IsNullOrWhiteSpace(personName) ? "Unknown" : personName,
            DateRange = rental.DateRangeDisplay,
            Status = rental.Status,
            TotalPrice = rental.TotalPriceDisplay
        };
    }
}
