using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Models;
using StarterApp.Services;

namespace StarterApp.ViewModels;

public partial class RentalsViewModel : BaseViewModel
{
    private readonly IRentalService _rentalService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<RentalListItem> incomingRentals = new();

    [ObservableProperty]
    private ObservableCollection<RentalListItem> outgoingRentals = new();

    [ObservableProperty]
    private string successMessage = string.Empty;

    public RentalsViewModel(
        IRentalService rentalService,
        INavigationService navigationService)
    {
        _rentalService = rentalService;
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

            var incoming = await _rentalService.GetIncomingAsync();
            var outgoing = await _rentalService.GetOutgoingAsync();

            IncomingRentals = new ObservableCollection<RentalListItem>(
                incoming.Select(rental => RentalListItem.FromRental(rental, "Borrower", RentalPerspective.Owner, _rentalService)));

            OutgoingRentals = new ObservableCollection<RentalListItem>(
                outgoing.Select(rental => RentalListItem.FromRental(rental, "Owner", RentalPerspective.Borrower, _rentalService)));
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

    [RelayCommand]
    private async Task ReviewRentalAsync(RentalListItem rental)
    {
        if (rental == null)
            return;

        var itemTitle = Uri.EscapeDataString(rental.ItemTitle);
        await _navigationService.NavigateToAsync($"ReviewsPage?rentalId={rental.Id}&itemId={rental.ItemId}&itemTitle={itemTitle}");
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

            await _rentalService.UpdateStatusAsync(
                rental.Id,
                rental.Status,
                status,
                rental.Perspective == RentalPerspective.Owner,
                rental.Perspective == RentalPerspective.Borrower,
                rental.StartDate);
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

    public int ItemId { get; set; }

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

    public bool CanApproveOrReject { get; set; }

    public bool CanMarkOutForRent { get; set; }

    public bool CanMarkReturned { get; set; }

    public bool CanComplete { get; set; }

    public bool CanReview =>
        Perspective == RentalPerspective.Borrower &&
        string.Equals(Status, "Completed", StringComparison.OrdinalIgnoreCase);

    public bool HasWorkflowAction => CanApproveOrReject || CanMarkOutForRent || CanMarkReturned || CanComplete || CanReview;

    public static RentalListItem FromRental(
        Rental rental,
        string personLabel,
        RentalPerspective perspective,
        IRentalService rentalService)
    {
        var personName = personLabel == "Owner" ? rental.OwnerName : rental.BorrowerName;
        var isOwnerAction = perspective == RentalPerspective.Owner;
        var isBorrowerAction = perspective == RentalPerspective.Borrower;

        return new RentalListItem
        {
            Id = rental.Id,
            ItemId = rental.ItemId,
            ItemTitle = rental.ItemTitle,
            PersonLabel = personLabel,
            PersonName = string.IsNullOrWhiteSpace(personName) ? "Unknown" : personName,
            DateRange = rental.DateRangeDisplay,
            StartDate = rental.StartDate,
            EndDate = rental.EndDate,
            Status = rental.Status,
            TotalPrice = rental.TotalPriceDisplay,
            Perspective = perspective,
            CanApproveOrReject =
                rentalService.CanTransition(rental.Status, "Approved", isOwnerAction, isBorrowerAction, rental.StartDate) ||
                rentalService.CanTransition(rental.Status, "Rejected", isOwnerAction, isBorrowerAction, rental.StartDate),
            CanMarkOutForRent = rentalService.CanTransition(rental.Status, "Out for Rent", isOwnerAction, isBorrowerAction, rental.StartDate),
            CanMarkReturned = rentalService.CanTransition(rental.Status, "Returned", isOwnerAction, isBorrowerAction, rental.StartDate),
            CanComplete = rentalService.CanTransition(rental.Status, "Completed", isOwnerAction, isBorrowerAction, rental.StartDate)
        };
    }
}

public enum RentalPerspective
{
    Owner,
    Borrower
}
