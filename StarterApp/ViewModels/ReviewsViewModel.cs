using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Models;
using StarterApp.Repositories;
using StarterApp.Services;

namespace StarterApp.ViewModels;

[QueryProperty(nameof(ItemId), "itemId")]
[QueryProperty(nameof(RentalId), "rentalId")]
[QueryProperty(nameof(ItemTitle), "itemTitle")]
public partial class ReviewsViewModel : BaseViewModel
{
    private readonly IReviewRepository _reviewRepository;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private int itemId;

    [ObservableProperty]
    private int rentalId;

    [ObservableProperty]
    private string itemTitle = string.Empty;

    [ObservableProperty]
    private double rating = 5;

    [ObservableProperty]
    private string comment = string.Empty;

    [ObservableProperty]
    private string successMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ReviewListItem> reviews = new();

    public ReviewsViewModel(IReviewRepository reviewRepository, INavigationService navigationService)
    {
        _reviewRepository = reviewRepository;
        _navigationService = navigationService;
        Title = "Reviews";
    }

    public bool CanSubmitReview => RentalId > 0;

    public bool CanViewItemReviews => ItemId > 0;

    public string RatingDisplay => $"{Math.Round(Rating):0}/5";

    partial void OnItemIdChanged(int value)
    {
        OnPropertyChanged(nameof(CanViewItemReviews));
        _ = LoadReviewsAsync();
    }

    partial void OnRentalIdChanged(int value)
    {
        OnPropertyChanged(nameof(CanSubmitReview));
    }

    partial void OnRatingChanged(double value)
    {
        OnPropertyChanged(nameof(RatingDisplay));
    }

    [RelayCommand]
    private async Task LoadReviewsAsync()
    {
        if (IsBusy || ItemId <= 0)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            var itemReviews = await _reviewRepository.GetForItemAsync(ItemId);
            Reviews = new ObservableCollection<ReviewListItem>(
                itemReviews.Select(ReviewListItem.FromReview));
        }
        catch (Exception ex)
        {
            SetError($"Failed to load reviews: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SubmitReviewAsync()
    {
        if (RentalId <= 0)
        {
            SetError("A completed rental is required before submitting a review.");
            return;
        }

        var ratingValue = (int)Math.Round(Rating);
        if (ratingValue < 1 || ratingValue > 5)
        {
            SetError("Rating must be between 1 and 5.");
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();
            SuccessMessage = string.Empty;

            await _reviewRepository.SubmitAsync(RentalId, ratingValue, Comment);
            SuccessMessage = "Review submitted.";
            Comment = string.Empty;
        }
        catch (Exception ex)
        {
            SetError($"Failed to submit review: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }

        await LoadReviewsAsync();
    }

    [RelayCommand]
    private async Task BackAsync()
    {
        await _navigationService.NavigateBackAsync();
    }
}

public class ReviewListItem
{
    public string ReviewerName { get; set; } = string.Empty;

    public int Rating { get; set; }

    public string RatingDisplay => $"{Rating}/5";

    public string Comment { get; set; } = string.Empty;

    public string CreatedAtDisplay { get; set; } = string.Empty;

    public static ReviewListItem FromReview(Review review)
    {
        return new ReviewListItem
        {
            ReviewerName = review.ReviewerName,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAtDisplay = review.CreatedAt.ToLocalTime().ToString("dd MMM yyyy")
        };
    }
}
