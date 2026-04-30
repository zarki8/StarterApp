using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class ReviewsPage : ContentPage
{
    public ReviewsPage(ReviewsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
