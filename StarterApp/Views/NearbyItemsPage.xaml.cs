using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class NearbyItemsPage : ContentPage
{
    public NearbyItemsPage(NearbyItemsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
