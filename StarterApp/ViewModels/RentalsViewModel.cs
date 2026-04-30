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

    [ObservableProperty]
    private string successMessage = string.Empty;

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
                incoming.Select(rental => RentalListItem.FromRental(rental, "Borrower", RentalPerspective.Owner)));

            OutgoingRentals = new ObservableCollection<RentalListItem>(
                outgoing.Select(rental => RentalListItem.FromRental(rental, "Owner", RentalPerspective.Borrower)));
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

    [RelayCommand]
    private async Task ApproveRentalAsync(RentalListItem rental)
    {
        await UpdateRentalStatusAsync(rental, "Approved");
    }

    [RelayCommand]
    private async Task RejectRentalAsync(RentalListItem rental)
    {
        await UpdateRentalStatusAsync(rental, "Rejected");
    }

    [RelayCommand]
    private async Task MarkOutForRentAsync(RentalListItem rental)
    {
        await UpdateRentalStatusAsync(rental, "Out for Rent");
    }

    [RelayCommand]
    private async Task MarkReturnedAsync(RentalListItem rental)
    {
        await UpdateRentalStatusAsync(rental, "Returned");
    }

    [RelayCommand]
    private async Task CompleteRentalAsync(RentalListItem rental)
    {
        await UpdateRentalStatusAsync(rental, "Completed");
    }

    private async Task UpdateRentalStatusAsync(RentalListItem rental, string status)
    {
        if (rental == null || IsBusy)
            return;

        try
        {
            IsBusy = true;
            ClearError();
            SuccessMessage = string.Empty;

            await _rentalRepository.UpdateStatusAsync(rental.Id, status);
            SuccessMessage = $"Rental {status.ToLowerInvariant()}.";
        }
        catch (Exception ex)
        {
            SetError($"Failed to update rental: {ex.Message}");
            return;
        }
        finally
        {
            IsBusy = false;
        }

        await LoadRentalsAsync();
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

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public string TotalPrice { get; set; } = string.Empty;

    public RentalPerspective Perspective { get; set; }

    public bool CanApproveOrReject =>
        Perspective == RentalPerspective.Owner &&
        string.Equals(Status, "Requested", StringComparison.OrdinalIgnoreCase);

    public bool CanMarkOutForRent =>
        Perspective == RentalPerspective.Owner &&
        string.Equals(Status, "Approved", StringComparison.OrdinalIgnoreCase) &&
        DateTime.Today >= StartDate.Date;

    public bool CanMarkReturned =>
        Perspective == RentalPerspective.Borrower &&
        (string.Equals(Status, "Out for Rent", StringComparison.OrdinalIgnoreCase) ||
         string.Equals(Status, "Overdue", StringComparison.OrdinalIgnoreCase));

    public bool CanComplete =>
        Perspective == RentalPerspective.Owner &&
        string.Equals(Status, "Returned", StringComparison.OrdinalIgnoreCase);

    public bool HasWorkflowAction => CanApproveOrReject || CanMarkOutForRent || CanMarkReturned || CanComplete;

    public static RentalListItem FromRental(Rental rental, string personLabel, RentalPerspective perspective)
    {
        var personName = personLabel == "Owner" ? rental.OwnerName : rental.BorrowerName;

        return new RentalListItem
        {
            Id = rental.Id,
            ItemTitle = rental.ItemTitle,
            PersonLabel = personLabel,
            PersonName = string.IsNullOrWhiteSpace(personName) ? "Unknown" : personName,
            DateRange = rental.DateRangeDisplay,
            StartDate = rental.StartDate,
            EndDate = rental.EndDate,
            Status = rental.Status,
            TotalPrice = rental.TotalPriceDisplay,
            Perspective = perspective
        };
    }
}

public enum RentalPerspective
{
    Owner,
    Borrower
}
