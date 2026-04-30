using StarterApp.Models;
using StarterApp.Repositories;

namespace StarterApp.Services;

public class RentalService : IRentalService
{
    private readonly IRentalRepository _rentalRepository;

    public RentalService(IRentalRepository rentalRepository)
    {
        _rentalRepository = rentalRepository;
    }

    public Task<Rental> RequestRentalAsync(int itemId, DateTime startDate, DateTime endDate)
    {
        ValidateRentalRequest(itemId, startDate, endDate);
        return _rentalRepository.RequestRentalAsync(itemId, startDate.Date, endDate.Date);
    }

    public Task<List<Rental>> GetIncomingAsync()
    {
        return _rentalRepository.GetIncomingAsync();
    }

    public Task<List<Rental>> GetOutgoingAsync()
    {
        return _rentalRepository.GetOutgoingAsync();
    }

    public Task UpdateStatusAsync(
        int rentalId,
        string currentStatus,
        string nextStatus,
        bool isOwnerAction,
        bool isBorrowerAction,
        DateTime startDate)
    {
        ValidateStatusTransition(currentStatus, nextStatus, isOwnerAction, isBorrowerAction, startDate);
        return _rentalRepository.UpdateStatusAsync(rentalId, nextStatus);
    }

    public bool CanTransition(
        string currentStatus,
        string nextStatus,
        bool isOwnerAction,
        bool isBorrowerAction,
        DateTime startDate)
    {
        try
        {
            ValidateStatusTransition(currentStatus, nextStatus, isOwnerAction, isBorrowerAction, startDate);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private static void ValidateRentalRequest(int itemId, DateTime startDate, DateTime endDate)
    {
        if (itemId <= 0)
        {
            throw new InvalidOperationException("An item is required before requesting a rental.");
        }

        if (startDate.Date < DateTime.Today)
        {
            throw new InvalidOperationException("Start date must be today or later.");
        }

        if (endDate.Date <= startDate.Date)
        {
            throw new InvalidOperationException("End date must be after the start date.");
        }
    }

    private static void ValidateStatusTransition(
        string currentStatus,
        string nextStatus,
        bool isOwnerAction,
        bool isBorrowerAction,
        DateTime startDate)
    {
        if (string.IsNullOrWhiteSpace(currentStatus) || string.IsNullOrWhiteSpace(nextStatus))
        {
            throw new InvalidOperationException("Rental status is required.");
        }

        if (IsTransition(currentStatus, "Requested", nextStatus, "Approved") ||
            IsTransition(currentStatus, "Requested", nextStatus, "Rejected"))
        {
            RequireOwner(isOwnerAction);
            return;
        }

        if (IsTransition(currentStatus, "Approved", nextStatus, "Out for Rent"))
        {
            RequireOwner(isOwnerAction);
            if (DateTime.Today < startDate.Date)
            {
                throw new InvalidOperationException("Rental cannot be marked out for rent before the start date.");
            }

            return;
        }

        if (IsTransition(currentStatus, "Out for Rent", nextStatus, "Returned") ||
            IsTransition(currentStatus, "Overdue", nextStatus, "Returned"))
        {
            if (!isBorrowerAction)
            {
                throw new InvalidOperationException("Only the borrower can mark an item as returned.");
            }

            return;
        }

        if (IsTransition(currentStatus, "Returned", nextStatus, "Completed"))
        {
            RequireOwner(isOwnerAction);
            return;
        }

        throw new InvalidOperationException($"Cannot transition from {currentStatus} to {nextStatus}.");
    }

    private static bool IsTransition(string currentStatus, string expectedCurrent, string nextStatus, string expectedNext)
    {
        return string.Equals(currentStatus, expectedCurrent, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(nextStatus, expectedNext, StringComparison.OrdinalIgnoreCase);
    }

    private static void RequireOwner(bool isOwnerAction)
    {
        if (!isOwnerAction)
        {
            throw new InvalidOperationException("Only the owner can perform this rental action.");
        }
    }
}
