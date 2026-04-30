using StarterApp.Models;

namespace StarterApp.Services;

public interface IRentalService
{
    Task<Rental> RequestRentalAsync(int itemId, DateTime startDate, DateTime endDate);

    Task<List<Rental>> GetIncomingAsync();

    Task<List<Rental>> GetOutgoingAsync();

    Task UpdateStatusAsync(
        int rentalId,
        string currentStatus,
        string nextStatus,
        bool isOwnerAction,
        bool isBorrowerAction,
        DateTime startDate);

    bool CanTransition(
        string currentStatus,
        string nextStatus,
        bool isOwnerAction,
        bool isBorrowerAction,
        DateTime startDate);
}
