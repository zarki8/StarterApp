using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class ItemListPage : ContentPage
{
    private readonly ItemListViewModel _viewModel;

    public ItemListPage(ItemListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadItemsCommand.Execute(null);
    }
}
