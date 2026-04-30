using StarterApp.Database.Models;
using StarterApp.Models;
using StarterApp.Repositories;
using StarterApp.Services;
using StarterApp.ViewModels;

namespace StarterApp.Test.ViewModels;

public class NearbyItemsViewModelTests
{
    [Fact]
    public async Task UseCurrentLocationAsync_MockedLocation_UpdatesCoordinateFields()
    {
        // Arrange
        var viewModel = new NearbyItemsViewModel(
            new FakeItemRepository(),
            new FakeLocationService(55.96, -3.19),
            new FakeNavigationService());

        // Act
        await viewModel.UseCurrentLocationCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal("55.96", viewModel.LatitudeText);
        Assert.Equal("-3.19", viewModel.LongitudeText);
        Assert.Equal("Current device location", viewModel.LocationSummary);
    }

    [Fact]
    public async Task SearchNearbyAsync_InvalidLatitude_SetsErrorAndDoesNotCallRepository()
    {
        // Arrange
        var repository = new FakeItemRepository();
        var viewModel = new NearbyItemsViewModel(
            repository,
            new FakeLocationService(55.96, -3.19),
            new FakeNavigationService())
        {
            LatitudeText = "999",
            LongitudeText = "-3.1883"
        };

        // Act
        await viewModel.SearchNearbyCommand.ExecuteAsync(null);

        // Assert
        Assert.True(viewModel.HasError);
        Assert.Equal("Latitude must be between -90 and 90.", viewModel.ErrorMessage);
        Assert.False(repository.NearbyWasCalled);
    }

    [Fact]
    public async Task SearchNearbyAsync_ValidCoordinates_LoadsNearbyItems()
    {
        // Arrange
        var repository = new FakeItemRepository();
        repository.NearbyItems.Add(new NearbyItem
        {
            Id = 7,
            Title = "Location test",
            DailyRate = 12,
            DistanceKm = 0.8m,
            Category = "Tools",
            OwnerName = "Admin Company"
        });

        var viewModel = new NearbyItemsViewModel(
            repository,
            new FakeLocationService(55.96, -3.19),
            new FakeNavigationService())
        {
            LatitudeText = "55.9533",
            LongitudeText = "-3.1883",
            RadiusKm = 5
        };

        // Act
        await viewModel.SearchNearbyCommand.ExecuteAsync(null);

        // Assert
        Assert.True(repository.NearbyWasCalled);
        Assert.Single(viewModel.Items);
        Assert.Equal("Location test", viewModel.Items[0].Title);
        Assert.Equal("0.8 km away", viewModel.Items[0].DistanceDisplay);
    }

    private sealed class FakeLocationService : ILocationService
    {
        private readonly UserLocation _location;

        public FakeLocationService(double latitude, double longitude)
        {
            _location = new UserLocation(latitude, longitude);
        }

        public Task<UserLocation> GetCurrentLocationAsync() => Task.FromResult(_location);
    }

    private sealed class FakeNavigationService : INavigationService
    {
        public Task NavigateToAsync(string route) => Task.CompletedTask;

        public Task NavigateToAsync(string route, Dictionary<string, object> parameters) => Task.CompletedTask;

        public Task NavigateBackAsync() => Task.CompletedTask;

        public Task NavigateToRootAsync() => Task.CompletedTask;

        public Task PopToRootAsync() => Task.CompletedTask;
    }

    private sealed class FakeItemRepository : IItemRepository
    {
        public bool NearbyWasCalled { get; private set; }
        public List<NearbyItem> NearbyItems { get; } = new();

        public Task<List<NearbyItem>> GetNearbyAsync(double latitude, double longitude, double radiusKm, string? category = null)
        {
            NearbyWasCalled = true;
            return Task.FromResult(NearbyItems);
        }

        public Task<Item> AddAsync(Item item) => Task.FromResult(item);

        public Task UpdateAsync(Item item) => Task.CompletedTask;

        public Task<List<Item>> GetAllAsync() => Task.FromResult(new List<Item>());

        public Task<Item?> GetByIdAsync(int id) => Task.FromResult<Item?>(null);
    }
}
