using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Repositories;
using StarterApp.Services;

namespace StarterApp.ViewModels;

public partial class ItemListViewModel : BaseViewModel
{
    private readonly IItemRepository _itemRepository;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<ItemListItem> items = new();

    public ItemListViewModel(
        IItemRepository itemRepository,
        INavigationService navigationService)
    {
        _itemRepository = itemRepository;
        _navigationService = navigationService;
        Title = "Items";
    }

    [RelayCommand]
    private async Task LoadItemsAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            var itemList = await _itemRepository.GetAllAsync();

            Items = new ObservableCollection<ItemListItem>(
                itemList.Select(item => new ItemListItem
                {
                    Id = item.Id,
                    Title = item.Title,
                    Description = item.Description,
                    DailyRate = item.DailyRate,
                    Category = item.Category,
                    Location = item.Location,
                    OwnerName = item.Owner?.FullName ?? "Unknown owner"
                }));
        }
        catch (Exception ex)
        {
            SetError($"Failed to load items: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ViewItemAsync(ItemListItem item)
    {
        if (item == null)
            return;

        await _navigationService.NavigateToAsync($"ItemDetailPage?itemId={item.Id}");
    }

    [RelayCommand]
    private async Task CreateItemAsync()
    {
        await _navigationService.NavigateToAsync("ItemDetailPage?itemId=0");
    }

    [RelayCommand]
    private async Task NavigateToDashboardAsync()
    {
        await _navigationService.NavigateToAsync("MainPage");
    }
}

public class ItemListItem
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal DailyRate { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string OwnerName { get; set; } = string.Empty;

    public string DailyRateDisplay => $"£{DailyRate:0.00}/day";
}
