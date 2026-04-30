using StarterApp.Models;
using StarterApp.Repositories;
using StarterApp.Services;

namespace StarterApp.Test.Services;

public class RentalServiceTests
{
    [Fact]
    public async Task RequestRentalAsync_EndDateSameAsStart_ThrowsValidationError()
    {
        // Arrange
        var service = new RentalService(new FakeRentalRepository());
        var startDate = DateTime.Today;

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RequestRentalAsync(42, startDate, startDate));

        // Assert
        Assert.Equal("End date must be after the start date.", exception.Message);
    }

    [Fact]
    public async Task RequestRentalAsync_ValidDates_CallsRepository()
    {
        // Arrange
        var repository = new FakeRentalRepository();
        var service = new RentalService(repository);

        // Act
        await service.RequestRentalAsync(42, DateTime.Today, DateTime.Today.AddDays(2));

        // Assert
        Assert.Equal(42, repository.RequestedItemId);
        Assert.True(repository.RequestWasCalled);
    }

    [Theory]
    [InlineData("Requested", "Approved", true, false, true)]
    [InlineData("Requested", "Rejected", true, false, true)]
    [InlineData("Approved", "Out for Rent", true, false, true)]
    [InlineData("Out for Rent", "Returned", false, true, true)]
    [InlineData("Returned", "Completed", true, false, true)]
    [InlineData("Approved", "Rejected", true, false, false)]
    [InlineData("Requested", "Approved", false, true, false)]
    public void CanTransition_StatusAndRole_ReturnsExpectedResult(
        string currentStatus,
        string nextStatus,
        bool isOwnerAction,
        bool isBorrowerAction,
        bool expected)
    {
        // Arrange
        var service = new RentalService(new FakeRentalRepository());

        // Act
        var canTransition = service.CanTransition(
            currentStatus,
            nextStatus,
            isOwnerAction,
            isBorrowerAction,
            DateTime.Today);

        // Assert
        Assert.Equal(expected, canTransition);
    }

    [Fact]
    public async Task UpdateStatusAsync_ApprovedBeforeStartDate_ThrowsValidationError()
    {
        // Arrange
        var repository = new FakeRentalRepository();
        var service = new RentalService(repository);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateStatusAsync(1, "Approved", "Out for Rent", true, false, DateTime.Today.AddDays(1)));

        // Assert
        Assert.Equal("Rental cannot be marked out for rent before the start date.", exception.Message);
        Assert.False(repository.StatusWasUpdated);
    }

    private sealed class FakeRentalRepository : IRentalRepository
    {
        public bool RequestWasCalled { get; private set; }
        public bool StatusWasUpdated { get; private set; }
        public int RequestedItemId { get; private set; }

        public Task<Rental> RequestRentalAsync(int itemId, DateTime startDate, DateTime endDate)
        {
            RequestWasCalled = true;
            RequestedItemId = itemId;
            return Task.FromResult(new Rental { Id = 1, ItemId = itemId, StartDate = startDate, EndDate = endDate });
        }

        public Task<List<Rental>> GetIncomingAsync(string? status = null) => Task.FromResult(new List<Rental>());

        public Task<List<Rental>> GetOutgoingAsync(string? status = null) => Task.FromResult(new List<Rental>());

        public Task UpdateStatusAsync(int rentalId, string status)
        {
            StatusWasUpdated = true;
            return Task.CompletedTask;
        }

        public Task<List<Rental>> GetAllAsync() => Task.FromResult(new List<Rental>());

        public Task<Rental?> GetByIdAsync(int id) => Task.FromResult<Rental?>(null);
    }
}
