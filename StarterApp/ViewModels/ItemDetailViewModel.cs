using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Models;
using StarterApp.Repositories;
using StarterApp.Services;

namespace StarterApp.ViewModels;

[QueryProperty(nameof(ItemId), "itemId")]
public partial class ItemDetailViewModel : BaseViewModel
{
    private readonly IItemRepository _itemRepository;
    private readonly IAuthenticationService _authService;
    private readonly INavigationService _navigationService;

    private Item? currentItem;

    [ObservableProperty]
    private int itemId = -1;

    [ObservableProperty]
    private string itemTitle = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private decimal dailyRate;

    [ObservableProperty]
    private string category = string.Empty;

    [ObservableProperty]
    private string location = string.Empty;

    [ObservableProperty]
    private string ownerName = string.Empty;

    [ObservableProperty]
    private bool isOwner;

    [ObservableProperty]
    private bool isNewItem;

    [ObservableProperty]
    private string successMessage = string.Empty;

    public string PageTitle => IsNewItem ? "Create Item" : "Item Details";

    public bool CanEdit => IsNewItem || IsOwner;

    public ItemDetailViewModel(
        IItemRepository itemRepository,
        IAuthenticationService authService,
        INavigationService navigationService)
    {
        _itemRepository = itemRepository;
        _authService = authService;
        _navigationService = navigationService;
        Title = "Item Details";
    }

    partial void OnItemIdChanged(int value)
    {
        _ = LoadItemAsync();
    }

    partial void OnIsNewItemChanged(bool value)
    {
        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(CanEdit));
    }

    partial void OnIsOwnerChanged(bool value)
    {
        OnPropertyChanged(nameof(CanEdit));
    }

    [RelayCommand]
    private async Task LoadItemAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ClearError();
            SuccessMessage = string.Empty;

            if (ItemId == 0)
            {
                currentItem = null;
                IsNewItem = true;
                IsOwner = true;
                ItemTitle = string.Empty;
                Description = string.Empty;
                DailyRate = 0;
                Category = string.Empty;
                Location = string.Empty;
                OwnerName = _authService.CurrentUser?.FullName ?? "You";
                return;
            }

            IsNewItem = false;

            var item = await _itemRepository.GetByIdAsync(ItemId);
            if (item == null)
            {
                SetError("Item not found.");
                return;
            }

            currentItem = item;
            ItemTitle = item.Title;
            Description = item.Description;
            DailyRate = item.DailyRate;
            Category = item.Category;
            Location = item.Location;
            OwnerName = item.Owner?.FullName ?? "Unknown owner";
            IsOwner = _authService.CurrentUser?.Id == item.OwnerId;
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
    private async Task SaveItemAsync()
    {
        if (!ValidateInput())
            return;

        if (!CanEdit)
        {
            SetError("Only the owner can update this item.");
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();
            SuccessMessage = string.Empty;

            if (IsNewItem)
            {
                await CreateItemAsync();
                SuccessMessage = "Item created successfully.";
            }
            else
            {
                await UpdateItemAsync();
                SuccessMessage = "Item updated successfully.";
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to save item: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task BackAsync()
    {
        await _navigationService.NavigateToAsync("ItemListPage");
    }

    [RelayCommand]
    private async Task NavigateToDashboardAsync()
    {
        await _navigationService.NavigateToAsync("MainPage");
    }

    private async Task CreateItemAsync()
    {
        var currentUser = _authService.CurrentUser;
        if (currentUser == null)
        {
            SetError("You must be logged in to create an item.");
            return;
        }

        var item = new Item
        {
            Title = ItemTitle.Trim(),
            Description = Description.Trim(),
            DailyRate = DailyRate,
            Category = Category.Trim(),
            Location = Location.Trim(),
            OwnerId = currentUser.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        currentItem = await _itemRepository.AddAsync(item);
        ItemId = currentItem.Id;
        IsNewItem = false;
        IsOwner = true;
    }

    private async Task UpdateItemAsync()
    {
        if (currentItem == null)
            return;

        currentItem.Title = ItemTitle.Trim();
        currentItem.Description = Description.Trim();
        currentItem.DailyRate = DailyRate;
        currentItem.Category = Category.Trim();
        currentItem.Location = Location.Trim();
        currentItem.UpdatedAt = DateTime.UtcNow;

        await _itemRepository.UpdateAsync(currentItem);
    }

    private bool ValidateInput()
    {
        ClearError();

        if (string.IsNullOrWhiteSpace(ItemTitle) || ItemTitle.Trim().Length < 5)
        {
            SetError("Title must be at least 5 characters.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            SetError("Description is required.");
            return false;
        }

        if (DailyRate <= 0)
        {
            SetError("Daily rate must be greater than zero.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Category))
        {
            SetError("Category is required.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Location))
        {
            SetError("Location is required.");
            return false;
        }

        var locationParts = Location.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (locationParts.Length != 2 ||
            !decimal.TryParse(locationParts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var latitude) ||
            !decimal.TryParse(locationParts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var longitude) ||
            latitude < -90 ||
            latitude > 90 ||
            longitude < -180 ||
            longitude > 180)
        {
            SetError("Location must be coordinates like 55.9533, -3.1883.");
            return false;
        }

        return true;
    }
}
